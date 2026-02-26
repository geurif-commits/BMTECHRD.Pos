namespace BMTECHRD.Pos.Application.DTOs;
public sealed class PaymentsSummaryDto
{
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal Transfer { get; set; }
    public decimal Mixed { get; set; }
    public decimal Total { get; set; }
}
