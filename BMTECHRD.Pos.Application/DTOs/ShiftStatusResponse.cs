namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ShiftStatusResponse
{
    public System.Guid? ShiftId { get; set; }
    public string Status { get; set; } = "NONE"; // OPEN or NONE
    public System.DateTime? OpenedAt { get; set; }
    public string? OpenedByUsername { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal SalesCash { get; set; }
    public decimal SalesCard { get; set; }
    public decimal SalesTransfer { get; set; }
    public decimal SalesMixed { get; set; }
    public decimal TotalPayments { get; set; }
}
