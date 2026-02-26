using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class License : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public required Business Business { get; set; }

    public LicensePlan Plan { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.INACTIVE;

    // Clave única que activa el plan
    public required string ActivationKey { get; set; }

    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // null si LIFETIME

    public bool IsActive(DateTime utcNow)
        => Status == LicenseStatus.ACTIVE && (ExpiresAt == null || ExpiresAt > utcNow);
}