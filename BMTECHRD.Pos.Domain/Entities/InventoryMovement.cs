using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class InventoryMovement : Entity
{
    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int QuantityDelta { get; set; }

    public required string Reason { get; set; }

    public Guid? ActorUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}