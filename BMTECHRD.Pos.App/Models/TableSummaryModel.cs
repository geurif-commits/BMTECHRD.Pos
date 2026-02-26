using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class TableSummaryModel
{
    public Guid TableId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal CurrentTotal { get; set; }
    public int ItemsCount { get; set; }
}
