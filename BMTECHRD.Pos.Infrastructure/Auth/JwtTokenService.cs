using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BMTECHRD.Pos.Infrastructure.Auth;

/// <summary>
/// Implementación de JWT tokens con refresh token rotation + reuse detection + device binding (ETAPA 9 HARDENED).
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;
    private readonly byte[] _signingKeyBytes;

    public JwtTokenService(IConfiguration config, AppDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;

        _issuer = _config["Jwt:Issuer"] ?? "BMTECHRD.Pos.Api";
        _audience = _config["Jwt:Audience"] ?? "BMTECHRD.Pos.App";
        _accessTokenMinutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "30");
        _refreshTokenDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30");

        var signingKey = _config["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey not configured");
        if (signingKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey debe tener al menos 32 caracteres");

        _signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);
    }

    public string CreateAccessToken(User user, Guid businessId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", user.Id.ToString()),
            new("bid", businessId.ToString()),
            new("role", user.Role.ToString()),
            new("uname", user.Username),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_signingKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<(string RawToken, RefreshToken Entity)> CreateRefreshTokenAsync(
        User user,
        Guid businessId,
        string? deviceId = null,
        string? ipAddress = null)
    {
        using var rng = RandomNumberGenerator.Create();

        var tokenBytes = new byte[64];
        rng.GetBytes(tokenBytes);
        var rawToken = Convert.ToBase64String(tokenBytes);

        var tokenHash = HashToken(rawToken);

        var refreshToken = new RefreshToken
        {
            BusinessId = businessId,
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
            RevokedAt = null,
            ReplacedByTokenId = null,
            DeviceId = deviceId,
            IpAddress = ipAddress
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return (rawToken, refreshToken);
    }

    /// <summary>
    /// Validación simple (compatibilidad): si está activo retorna token, si no null.
    /// </summary>
    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string rawToken)
    {
        var detailed = await ValidateRefreshTokenDetailedAsync(rawToken, deviceIdFromRequest: null);
        return detailed.IsValid ? detailed.Token : null;
    }

    /// <summary>
    /// Validación detallada (ETAPA 9 HARDENED):
    /// - EXPIRED / REVOKED / NOT_FOUND / EMPTY
    /// - REUSE_DETECTED: token revocado + ReplacedByTokenId != null => incidente + kill switch
    /// - DEVICE_MISMATCH: request trae deviceId, token tiene deviceId y no coincide => incidente + kill switch
    /// </summary>
    public async Task<RefreshTokenValidationResult> ValidateRefreshTokenDetailedAsync(string rawToken, string? deviceIdFromRequest)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return new RefreshTokenValidationResult(false, false, "EMPTY", null);

        var tokenHash = HashToken(rawToken);

        // AsNoTracking: esta operación es solo lectura (y más rápida)
        var token = await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (token == null)
            return new RefreshTokenValidationResult(false, false, "NOT_FOUND", null);

        if (DateTime.UtcNow >= token.ExpiresAt)
            return new RefreshTokenValidationResult(false, false, "EXPIRED", null);

        // Si está revocado
        if (token.RevokedAt != null)
        {
            // Reuse detection: era un token rotado (replaced) y alguien lo reusó
            if (token.ReplacedByTokenId != null)
            {
                await RevokeAllRefreshTokensAsync(token.UserId, token.BusinessId, "REUSE_DETECTED");
                return new RefreshTokenValidationResult(false, true, "REUSE_DETECTED", null);
            }

            return new RefreshTokenValidationResult(false, false, "REVOKED", null);
        }

        // Device binding: aplica solo si el request envía deviceId y el token ya estaba “bound”
        // (tokens viejos con DeviceId null pasan, y se “atan” en el próximo refresh al crear uno nuevo)
        if (!string.IsNullOrWhiteSpace(deviceIdFromRequest) &&
            token.DeviceId != null &&
            !string.Equals(token.DeviceId, deviceIdFromRequest, StringComparison.Ordinal))
        {
            await RevokeAllRefreshTokensAsync(token.UserId, token.BusinessId, "DEVICE_MISMATCH");
            return new RefreshTokenValidationResult(false, true, "DEVICE_MISMATCH", null);
        }

        return new RefreshTokenValidationResult(true, false, null, token);
    }

    public async Task RevokeRefreshTokenAsync(Guid tokenId, Guid? replacedByTokenId = null)
    {
        var token = await _dbContext.RefreshTokens.FindAsync(tokenId);
        if (token == null) return;

        // idempotente: si ya estaba revocado, no hace falta re-escribir
        if (token.RevokedAt == null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenId = replacedByTokenId;
            await _dbContext.SaveChangesAsync();
        }
        else if (replacedByTokenId != null && token.ReplacedByTokenId == null)
        {
            // permite completar el enlace de rotación si faltaba
            token.ReplacedByTokenId = replacedByTokenId;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<int> RevokeAllRefreshTokensAsync(Guid userId, Guid businessId, string reason)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.BusinessId == businessId && t.RevokedAt == null)
            .ToListAsync();

        if (tokens.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        foreach (var t in tokens)
        {
            t.RevokedAt = now;
        }

        await _dbContext.SaveChangesAsync();
        return tokens.Count;
    }

    public string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(hashedBytes);
    }
}