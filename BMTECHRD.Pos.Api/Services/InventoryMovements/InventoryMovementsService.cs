using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.InventoryMovements;

public sealed class InventoryMovementsService : IInventoryMovementsService
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public InventoryMovementsService(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    public async Task<List<InventoryMovementDto>> GetMovementsAsync(Guid businessId, Guid? productId, DateTime? from, DateTime? to, int? limit, CancellationToken ct)
    {
        var take = limit ?? 200;
        if (take <= 0) take = 200;
        if (take > 1000) take = 1000;

        var query = _ctx.InventoryMovements.AsNoTracking().Where(m => m.BusinessId == businessId);
        if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
        if (from.HasValue) query = query.Where(m => m.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(m => m.CreatedAt <= to.Value);

        return await query.OrderByDescending(m => m.CreatedAt).Take(take)
            .Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product != null ? m.Product.Name : string.Empty,
                QuantityDelta = m.QuantityDelta,
                Reason = m.Reason,
                ActorUserId = m.ActorUserId,
                ActorUsername = m.ActorUserId.HasValue
                    ? _ctx.Users.Where(u => u.Id == m.ActorUserId).Select(u => u.Username).FirstOrDefault()
                    : null,
                CreatedAt = m.CreatedAt
            }).ToListAsync(ct);
    }

    public async Task<List<StockItemDto>> GetStockAsync(Guid businessId, CancellationToken ct)
    {
        var products = await _ctx.Products.AsNoTracking().Where(p => p.BusinessId == businessId).ToListAsync(ct);
        return products.Select(p => new StockItemDto
        {
            ProductId = p.Id,
            Name = p.Name,
            Stock = p.Stock,
            TrackInventory = p.TrackInventory
        }).ToList();
    }

    public async Task<(Guid ProductId, int Stock)> AdjustAsync(InventoryAdjustRequest req, CancellationToken ct)
    {
        var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
        if (actor == null || actor.BusinessId != req.BusinessId)
            throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor not allowed", "INV_ACTOR_FORBIDDEN");
        if (actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERVISOR)
            throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor role not allowed", "INV_ACTOR_ROLE_FORBIDDEN");

        var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId && p.BusinessId == req.BusinessId, ct);
        if (product == null)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Product not found", "Product not found", "INV_PRODUCT_NOT_FOUND");
        if (req.QuantityDelta == 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid adjustment", "QuantityDelta must be non-zero", "INV_ADJUST_ZERO");

        if (product.TrackInventory && product.Stock + req.QuantityDelta < 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Insufficient stock", "Insufficient stock for adjustment", "INV_STOCK_INSUFFICIENT");

        product.Stock += req.QuantityDelta;
        _ctx.InventoryMovements.Add(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ProductId = req.ProductId,
            QuantityDelta = req.QuantityDelta,
            Reason = req.Reason,
            ActorUserId = req.ActorUserId,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("inventory.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated", cancellationToken: ct);

        return (product.Id, product.Stock);
    }
}
