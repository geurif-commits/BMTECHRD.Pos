using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class ShiftSummaryModel
{
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal SalesCash { get; set; }
    public decimal SalesCard { get; set; }
    public decimal SalesTransfer { get; set; }
    public decimal SalesMixed { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal Difference { get; set; }

    public bool HasDifference => Difference != 0;
    public string DifferenceColor => HasDifference ? "#FF3D3D" : "#4CAF50"; // Red or Green
}
