using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class StockItemModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool TrackInventory { get; set; }
}
