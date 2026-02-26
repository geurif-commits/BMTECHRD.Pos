using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class InventoryAdjustRequestModel
{
    public Guid BusinessId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityDelta { get; set; }
    public string Reason { get; set; } = "AJUSTE";
    public Guid ActorUserId { get; set; }
}
