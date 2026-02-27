using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Orders;

public sealed class OrderBatchService : IOrderBatchService
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public OrderBatchService(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    public async Task<CreateOrderBatchResponse> CreateBatchAsync(CreateOrderBatchRequest req, CancellationToken ct)
    {
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

        int kitchen = 0, bar = 0, total = 0;

        foreach (var line in req.Items)
        {
            var prod = products[line.ProductId];
            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                BusinessId = req.BusinessId,
                OrderId = order.Id,
                ProductId = prod.Id,
                ProductNameSnapshot = prod.Name,
                UnitPriceSnapshot = prod.Price,
                Quantity = line.Quantity,
                Area = prod.Area,
                Status = OrderItemStatus.SENT,
                CreatedAt = DateTime.UtcNow
            };
            _ctx.OrderItems.Add(item);

            if (prod.TrackInventory)
            {
                prod.Stock -= line.Quantity;
                var mov = new InventoryMovement
                {
                    Id = Guid.NewGuid(),
                    BusinessId = req.BusinessId,
                    ProductId = prod.Id,
                    QuantityDelta = -line.Quantity,
                    Reason = "SALE",
                    ActorUserId = req.ActorUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _ctx.InventoryMovements.Add(mov);
            }

            if (prod.Area == ProductionArea.KITCHEN) kitchen += line.Quantity;
            if (prod.Area == ProductionArea.BAR) bar += line.Quantity;
            total += line.Quantity;
        }

        await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        if (kitchen > 0) await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("kitchen.queue.updated", cancellationToken: ct);
        if (bar > 0) await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("bar.queue.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("inventory.updated", cancellationToken: ct);

        return new CreateOrderBatchResponse
        {
            OrderId = order.Id,
            TotalItems = total,
            KitchenItems = kitchen,
            BarItems = bar
        };
    }
}
