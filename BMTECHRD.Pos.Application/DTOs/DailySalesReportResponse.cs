namespace BMTECHRD.Pos.Application.DTOs;
public sealed class DailySalesReportResponse
{
    public required System.Collections.Generic.List<SalesDailyDto> Items { get; set; }
    public required PaymentsSummaryDto Payments { get; set; }
}
