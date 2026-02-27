using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Cashier;

public interface ICashierService
{
    Task<List<TableSummaryDto>> GetOpenTablesAsync(Guid businessId, CancellationToken ct);
    Task<GetBillResponse> GetBillAsync(Guid businessId, Guid tableId, CancellationToken ct);
    Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest req, string? idempotencyKey, CancellationToken ct);
    Task<bool> CloseTableAsync(CloseTableRequest req, CancellationToken ct);
}
