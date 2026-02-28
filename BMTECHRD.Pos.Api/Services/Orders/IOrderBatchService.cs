using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Orders;

public interface IOrderBatchService
{
    Task<CreateOrderBatchResponse> CreateBatchAsync(CreateOrderBatchRequest req, string? idempotencyKey, CancellationToken ct);
}
