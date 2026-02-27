using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Orders;

public sealed class OrderBatchService : IOrderBatchService
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    private const string IdempotencyScope = "ORDER_BATCH_CREATE";

    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;
    private readonly IIdempotencyKeyStore _idempotencyKeyStore;

    public OrderBatchService(AppDbContext ctx, IHubContext<PosHub> hub, IIdempotencyKeyStore idempotencyKeyStore)
    {
        _ctx = ctx;
        _hub = hub;
        _idempotencyKeyStore = idempotencyKeyStore;
    }

    public async Task<CreateOrderBatchResponse> CreateBatchAsync(CreateOrderBatchRequest req, string? idempotencyKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingOrderId = await _idempotencyKeyStore.TryGetEntityIdAsync(IdempotencyScope, req.BusinessId, req.ActorUserId, idempotencyKey, ct);
            if (existingOrderId.HasValue)
            {
                var existingItems = await _ctx.OrderItems.AsNoTracking().Where(oi => oi.OrderId == existingOrderId.Value).ToListAsync(ct);
                if (existingItems.Count > 0)
                {
                    return new CreateOrderBatchResponse
                    {
                        OrderId = existingOrderId.Value,
                        TotalItems = existingItems.Sum(i => i.Quantity),
                        KitchenItems = existingItems.Where(i => i.Area == ProductionArea.KITCHEN).Sum(i => i.Quantity),
                        BarItems = existingItems.Where(i => i.Area == ProductionArea.BAR).Sum(i => i.Quantity)
                    };
                }
            }
        }

        var table = await _ctx.Tables.FindAsync(new object?[] { req.TableId }, ct);
        if (table == null || table.BusinessId != req.BusinessId)
            throw new ApiProblemException(400, "Table not found", "Table not found for business", "ORDER_TABLE_NOT_FOUND");

        if (table.Status != TableStatus.OPEN)
            throw new ApiProblemException(400, "Invalid table status", "Table must be OPEN to place order", "ORDER_TABLE_NOT_OPEN");

        if (req.Items == null || req.Items.Count == 0)
            throw new ApiProblemException(400, "Invalid order", "No items", "ORDER_EMPTY");

        var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _ctx.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var line in req.Items)
        {
            if (!products.ContainsKey(line.ProductId))
                throw new ApiProblemException(400, "Product not found", $"Product {line.ProductId} not found", "ORDER_PRODUCT_NOT_FOUND");

            if (line.Quantity <= 0)
                throw new ApiProblemException(400, "Invalid quantity", "Invalid quantity", "ORDER_INVALID_QUANTITY");

            var prod = products[line.ProductId];
            if (prod.TrackInventory && prod.Stock < line.Quantity)
                throw new ApiProblemException(400, "Insufficient stock", $"Insufficient stock for product {prod.Name}", "ORDER_STOCK_INSUFFICIENT");
        }

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            TableId = req.TableId,
            CreatedByUserId = req.ActorUserId,
            CreatedAt = DateTime.UtcNow
        };
        _ctx.Orders.Add(order);

        var totalItems = 0;
        var kitchenItems = 0;
        var barItems = 0;

        foreach (var line in req.Items)
        {
            var prod = products[line.ProductId];
            var area = prod.Area;

            var oi = new OrderItem
            {
                Id = Guid.NewGuid(),
                BusinessId = req.BusinessId,
                OrderId = order.Id,
                ProductId = prod.Id,
                ProductNameSnapshot = prod.Name,
                UnitPriceSnapshot = prod.Price,
                Quantity = line.Quantity,
                Area = area,
                Status = OrderItemStatus.SENT,
                Notes = line.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _ctx.OrderItems.Add(oi);

            if (prod.TrackInventory)
                prod.Stock -= line.Quantity;

            _ctx.InventoryMovements.Add(new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = req.BusinessId,
                ProductId = prod.Id,
                QuantityDelta = -line.Quantity,
                Reason = "Order item created",
                ActorUserId = req.ActorUserId,
                CreatedAt = DateTime.UtcNow
            });

            totalItems += line.Quantity;
            if (area == ProductionArea.KITCHEN) kitchenItems += line.Quantity;
            if (area == ProductionArea.BAR) barItems += line.Quantity;
        }

        table.UpdatedAt = DateTime.UtcNow;

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ActorUserId = req.ActorUserId,
            Action = "ORDER_BATCH_CREATE",
            EntityType = "Order",
            EntityId = order.Id,
            DataJson = $"items={req.Items.Count}",
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _idempotencyKeyStore.SaveAsync(IdempotencyScope, req.BusinessId, req.ActorUserId, idempotencyKey, order.Id, IdempotencyTtl, ct);
        }

        await tx.CommitAsync(ct);

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("orders.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("inventory.updated", cancellationToken: ct);

        return new CreateOrderBatchResponse
        {
            OrderId = order.Id,
            TotalItems = totalItems,
            KitchenItems = kitchenItems,
            BarItems = barItems
        };
    }
}
