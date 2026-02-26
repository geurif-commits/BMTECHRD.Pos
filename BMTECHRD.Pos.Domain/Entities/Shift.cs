using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Shift : AuditableEntity
{
    public Guid BusinessId { get; set; }
    // The user (cashier) who opened the shift
    public Guid UserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public string Status { get; set; } = "OPEN"; // OPEN/CLOSED
    public string? Notes { get; set; }
}
