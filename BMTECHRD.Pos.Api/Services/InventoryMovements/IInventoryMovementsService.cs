using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.InventoryMovements;

public interface IInventoryMovementsService
{
    Task<List<InventoryMovementDto>> GetMovementsAsync(Guid businessId, Guid? productId, DateTime? from, DateTime? to, int? limit, CancellationToken ct);
    Task<List<StockItemDto>> GetStockAsync(Guid businessId, CancellationToken ct);
    Task<(Guid ProductId, int Stock)> AdjustAsync(InventoryAdjustRequest req, CancellationToken ct);
}
