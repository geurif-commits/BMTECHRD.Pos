using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class BillLineModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
