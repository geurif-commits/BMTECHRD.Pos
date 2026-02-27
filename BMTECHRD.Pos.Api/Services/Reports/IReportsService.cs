using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Reports;

public interface IReportsService
{
    Task<DailySalesReportResponse> GetSalesDailyAsync(Guid businessId, Guid userId, DateTime from, DateTime to, CancellationToken ct);
    Task<List<SalesByProductDto>> GetSalesByProductAsync(Guid businessId, Guid userId, DateTime from, DateTime to, int? limit, CancellationToken ct);
    Task<List<SalesByUserDto>> GetSalesByUserAsync(Guid businessId, Guid userId, DateTime from, DateTime to, CancellationToken ct);
}
