using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

/// <summary>
/// Representa un token de refresco seguro con rotación y seguimiento de dispositivo/IP.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>
    /// Id del negocio propietario del token.
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Id del usuario propietario del token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hash SHA256 del token raw (se guarda en DB, nunca el token en claro).
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del token.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de expiración del token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Fecha de revocación (null si aún válido).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Si fue rotado, referencia al token que lo reemplazó.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Identificador del dispositivo (opcional, para seguimiento multi-device).
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Dirección IP de origen (opcional, para auditoría).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Indica si el token está activo (no revocado, no expirado).
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}
