namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ShiftSummaryDto
{
    public required System.Guid ShiftId { get; set; }
    public required System.Guid UserId { get; set; }
    public required string Username { get; set; }

    public required System.DateTime OpenedAt { get; set; }
    public System.DateTime? ClosedAt { get; set; }
    public required string Status { get; set; }

    public decimal SalesCash { get; set; }
    public decimal SalesCard { get; set; }
    public decimal SalesTransfer { get; set; }
    public decimal SalesMixed { get; set; }
    public decimal TotalPayments { get; set; }

    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? Difference { get; set; }
}
