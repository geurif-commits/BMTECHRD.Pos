using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext ctx, IPasswordHasher hasher, ITokenService tokenService)
    {
        _ctx = ctx;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest req, string? ipAddress, CancellationToken ct)
    {
        var business = await _ctx.Businesses.FindAsync(new object?[] { req.BusinessId }, ct);
        if (business == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Business not found", "Business not found", "AUTH_BUSINESS_NOT_FOUND");

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.BusinessId == req.BusinessId && u.Username == req.Username, ct);
        if (user == null)
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Invalid credentials", "Invalid credentials", "AUTH_INVALID_CREDENTIALS");

        var ok = _hasher.Verify(req.Password, user.PasswordHash ?? string.Empty);
        if (!ok)
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Invalid credentials", "Invalid credentials", "AUTH_INVALID_CREDENTIALS");

        var accessToken = _tokenService.CreateAccessToken(user, req.BusinessId);
        var (refreshTokenRaw, refreshTokenEntity) = await _tokenService.CreateRefreshTokenAsync(user, req.BusinessId, req.DeviceId, ipAddress);

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ActorUserId = user.Id,
            Action = "AUTH_LOGIN",
            EntityType = "User",
            EntityId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenRaw,
            UserId = user.Id,
            BusinessId = req.BusinessId,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest req, string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Refresh token is required", "Refresh token is required", "AUTH_REFRESH_TOKEN_REQUIRED");

        var validation = await _tokenService.ValidateRefreshTokenDetailedAsync(req.RefreshToken, req.DeviceId);

        if (validation.IsIncident)
        {
            _ctx.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                BusinessId = Guid.Empty,
                ActorUserId = Guid.Empty,
                Action = validation.Reason == "DEVICE_MISMATCH" ? "AUTH_REFRESH_DEVICE_MISMATCH" : "AUTH_REFRESH_REUSE_DETECTED",
                EntityType = "RefreshToken",
                EntityId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            });

            await _ctx.SaveChangesAsync(ct);

            throw new ApiProblemException(
                StatusCodes.Status401Unauthorized,
                "Invalid refresh token",
                validation.Reason == "DEVICE_MISMATCH" ? "Device mismatch. Please login again." : "Refresh token reuse detected. Please login again.",
                validation.Reason == "DEVICE_MISMATCH" ? "AUTH_DEVICE_MISMATCH" : "AUTH_REUSE_DETECTED");
        }

        if (!validation.IsValid || validation.Token == null)
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Invalid refresh token", "Invalid or expired refresh token", "AUTH_REFRESH_INVALID");

        var refreshToken = validation.Token;

        var user = await _ctx.Users.FindAsync(new object?[] { refreshToken.UserId }, ct);
        if (user == null)
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "User not found", "User not found", "AUTH_USER_NOT_FOUND");

        var newAccessToken = _tokenService.CreateAccessToken(user, refreshToken.BusinessId);
        var (newRefreshTokenRaw, newRefreshTokenEntity) = await _tokenService.CreateRefreshTokenAsync(
            user,
            refreshToken.BusinessId,
            req.DeviceId ?? refreshToken.DeviceId,
            ipAddress);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken.Id, newRefreshTokenEntity.Id);

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = refreshToken.BusinessId,
            ActorUserId = user.Id,
            Action = "AUTH_REFRESH",
            EntityType = "User",
            EntityId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenRaw,
            UserId = user.Id,
            BusinessId = refreshToken.BusinessId,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = newRefreshTokenEntity.ExpiresAt
        };
    }

    public async Task LogoutAsync(LogoutRequest req, string? subjectUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Refresh token is required", "Refresh token is required", "AUTH_REFRESH_TOKEN_REQUIRED");

        var refreshToken = await _tokenService.ValidateRefreshTokenAsync(req.RefreshToken);
        if (refreshToken == null)
            return;

        await _tokenService.RevokeRefreshTokenAsync(refreshToken.Id);

        if (!Guid.TryParse(subjectUserId, out var userIdGuid))
            return;

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = refreshToken.BusinessId,
            ActorUserId = userIdGuid,
            Action = "AUTH_LOGOUT",
            EntityType = "User",
            EntityId = userIdGuid,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);
    }

    public (Guid UserId, Guid BusinessId, string? Role, string? Username) Me(System.Security.Claims.ClaimsPrincipal user)
    {
        var userId = user.FindFirst("sub")?.Value;
        var businessId = user.FindFirst("bid")?.Value;
        var role = user.FindFirst("role")?.Value;
        var username = user.FindFirst("uname")?.Value;

        if (!Guid.TryParse(userId, out var userIdGuid) || !Guid.TryParse(businessId, out var businessIdGuid))
            throw new ApiProblemException(StatusCodes.Status401Unauthorized, "Unauthorized", "Invalid auth claims", "AUTH_INVALID_CLAIMS");

        return (userIdGuid, businessIdGuid, role, username);
    }
}
