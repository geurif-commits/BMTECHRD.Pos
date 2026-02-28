namespace BMTECHRD.Pos.Api.Services.Inventory;

public interface IInventoryService
{
    Task<InventoryAdjustResult> AdjustAsync(Guid businessId, Guid productId, int quantityDelta, string reason, Guid? actorUserId, CancellationToken ct);
}

public sealed record InventoryAdjustResult(Guid ProductId, int Stock);
