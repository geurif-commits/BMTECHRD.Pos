using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public OrdersController(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateOrderBatchRequest req)
    {
        // validate table
        var table = await _ctx.Tables.FindAsync(req.TableId);
        if (table == null || table.BusinessId != req.BusinessId) return BadRequest("Table not found for business");
        if (table.Status != BMTECHRD.Pos.Domain.Enums.TableStatus.OPEN) return BadRequest("Table must be OPEN to place order");

        if (req.Items == null || req.Items.Count == 0) return BadRequest("No items");

        var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _ctx.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        // validate products and stock
        foreach (var line in req.Items)
        {
            if (!products.ContainsKey(line.ProductId)) return BadRequest($"Product {line.ProductId} not found");
            if (line.Quantity <= 0) return BadRequest("Invalid quantity");
            var prod = products[line.ProductId];
            if (prod.TrackInventory && prod.Stock < line.Quantity) return BadRequest($"Insufficient stock for product {prod.Name}");
        }

        using var tx = await _ctx.Database.BeginTransactionAsync();
        try
        {
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
            var items = new List<OrderItem>();

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
                items.Add(item);
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

            await _ctx.SaveChangesAsync();
            await tx.CommitAsync();

            // emit events
            if (kitchen > 0) await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("kitchen.queue.updated");
            if (bar > 0) await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("bar.queue.updated");
            await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated");
            await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("inventory.updated");

            var resp = new CreateOrderBatchResponse { OrderId = order.Id, TotalItems = total, KitchenItems = kitchen, BarItems = bar };
            return Ok(resp);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
