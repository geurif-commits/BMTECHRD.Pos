using BMTECHRD.Pos.Domain.Entities;

namespace BMTECHRD.Pos.Application.Abstractions.Security;

public interface ITokenService
{
    string CreateAccessToken(User user, Guid businessId);

    Task<(string RawToken, RefreshToken Entity)> CreateRefreshTokenAsync(
        User user,
        Guid businessId,
        string? deviceId = null,
        string? ipAddress = null);

    /// <summary>
    /// Validación simple (compatibilidad): retorna el token si está activo.
    /// </summary>
    Task<RefreshToken?> ValidateRefreshTokenAsync(string rawToken);

    /// <summary>
    /// Validación detallada (ETAPA 9 HARDENED):
    /// - Reuse detection (REUSE_DETECTED)
    /// - Device binding (DEVICE_MISMATCH)
    /// - Retorna razón estructurada para que el Controller actúe correctamente.
    /// </summary>
    Task<RefreshTokenValidationResult> ValidateRefreshTokenDetailedAsync(string rawToken, string? deviceIdFromRequest);

    /// <summary>
    /// Revoca un refresh token. Si se rota, indicar replacedByTokenId.
    /// </summary>
    Task RevokeRefreshTokenAsync(Guid tokenId, Guid? replacedByTokenId = null);

    /// <summary>
    /// Kill switch: revoca todos los refresh tokens activos del usuario en un negocio.
    /// </summary>
    Task<int> RevokeAllRefreshTokensAsync(Guid userId, Guid businessId, string reason);

    /// <summary>
    /// Hash SHA256 de refresh token raw.
    /// </summary>
    string HashToken(string rawToken);
}

public sealed record RefreshTokenValidationResult(
    bool IsValid,
    bool IsIncident,
    string? Reason,
    RefreshToken? Token);