using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid BusinessId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }

    public required string ProductNameSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }

    public int Quantity { get; set; }

    public ProductionArea Area { get; set; }

    public OrderItemStatus Status { get; set; } = OrderItemStatus.SENT;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
