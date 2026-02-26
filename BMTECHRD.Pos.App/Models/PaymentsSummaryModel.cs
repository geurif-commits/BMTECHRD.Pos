namespace BMTECHRD.Pos.App.Models;

public sealed class PaymentsSummaryModel
{
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal Transfer { get; set; }
    public decimal Mixed { get; set; }
    public decimal Total { get; set; }
}
