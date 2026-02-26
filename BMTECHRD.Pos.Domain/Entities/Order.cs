using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Order : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Guid TableId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem>? Items { get; set; }
}
