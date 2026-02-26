using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class InventoryMovementModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public string? ActorUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeLabel => CreatedAt.ToString("g");
}
