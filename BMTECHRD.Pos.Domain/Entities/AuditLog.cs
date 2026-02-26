using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

/// <summary>
/// Registro de auditoría que documenta acciones críticas en el sistema.
/// </summary>
public sealed class AuditLog : Entity
{
    /// <summary>
    /// Id del negocio donde ocurrió la acción.
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Id del usuario que ejecutó la acción (puede ser null para acciones del sistema).
    /// </summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>
    /// Tipo de acción realizada (ej: "AUTH_LOGIN", "USER_CREATE", "SHIFT_OPEN", "ORDER_PAID").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de entidad afectada (ej: "User", "Shift", "Order", "Payment").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Id de la entidad afectada.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// JSON con datos relevantes (sin sensibles como password/PIN).
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// Marca de tiempo UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}