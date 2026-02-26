using System.Collections.ObjectModel;

namespace BMTECHRD.Pos.App.Models;

public sealed class DailySalesReportModel
{
    public ObservableCollection<SalesDailyItemModel> Items { get; } = new();
    public PaymentsSummaryModel Payments { get; set; } = new();
}
