using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Api.Services.Production;

public interface IProductionQueueService
{
    Task<List<ProductionQueueItemDto>> GetQueueAsync(Guid businessId, ProductionArea area, CancellationToken ct);
    Task<OrderItem> UpdateStatusAsync(Guid orderItemId, string status, ProductionArea area, CancellationToken ct);
}
