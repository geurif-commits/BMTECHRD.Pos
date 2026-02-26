using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class SalesDailyItemModel
{
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string DateLabel => Date.ToString("ddd, d MMM");
}
