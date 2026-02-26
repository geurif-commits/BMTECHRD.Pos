using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Payment : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Guid TableId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? MetaJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
