using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool TrackInventory { get; set; }

    public ProductionArea Area { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InventoryMovement>? InventoryMovements { get; set; }
}