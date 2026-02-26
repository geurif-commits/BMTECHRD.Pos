using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class SalesByProductModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Total { get; set; }
}
