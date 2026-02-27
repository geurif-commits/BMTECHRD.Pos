using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

namespace BMTECHRD.Pos.Api.Services.Inventory;

public sealed class InventoryService : IInventoryService
{
    private readonly AppDbContext _ctx;

    public InventoryService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<InventoryAdjustResult> AdjustAsync(Guid businessId, Guid productId, int quantityDelta, string reason, Guid? actorUserId, CancellationToken ct)
    {
        var product = await _ctx.Products.FindAsync(new object?[] { productId }, ct);
        if (product == null || product.BusinessId != businessId)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Product not found", "Product not found for business", "INV_PRODUCT_NOT_FOUND");

        if (product.TrackInventory && product.Stock + quantityDelta < 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Insufficient stock", "Insufficient stock", "INV_STOCK_INSUFFICIENT");

        product.Stock += quantityDelta;

        _ctx.InventoryMovements.Add(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ProductId = productId,
            QuantityDelta = quantityDelta,
            Reason = reason,
            ActorUserId = actorUserId,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        return new InventoryAdjustResult(product.Id, product.Stock);
    }
}
