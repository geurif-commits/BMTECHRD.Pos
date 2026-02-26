using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Bill : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Guid TableId { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Tip { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    public string Status { get; set; } = "OPEN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
}
