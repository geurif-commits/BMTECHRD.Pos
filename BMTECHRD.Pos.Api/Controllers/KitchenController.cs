using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/kitchen")]
public sealed class KitchenController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public KitchenController(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] Guid businessId)
    {
        var items = await _ctx.OrderItems
            .Where(i => i.BusinessId == businessId && i.Area == ProductionArea.KITCHEN && i.Status != OrderItemStatus.CANCELLED && i.Status != OrderItemStatus.DONE)
            .OrderBy(i => i.CreatedAt)
            .Join(_ctx.Orders, oi => oi.OrderId, o => o.Id, (oi, o) => new { oi, o })
            .Join(_ctx.Tables, x => x.o.TableId, t => t.Id, (x, t) => new ProductionQueueItemDto
            {
                OrderItemId = x.oi.Id,
                OrderId = x.o.Id,
                TableId = t.Id,
                TableNumber = t.Number,
                ProductName = x.oi.ProductNameSnapshot,
                Quantity = x.oi.Quantity,
                Status = x.oi.Status.ToString(),
                Area = x.oi.Area.ToString(),
                CreatedAt = x.oi.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPatch("items/{orderItemId}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid orderItemId, [FromBody] UpdateOrderItemStatusRequest req)
    {
        var item = await _ctx.OrderItems.FindAsync(orderItemId);
        if (item == null) return NotFound();
        if (item.Area != ProductionArea.KITCHEN) return BadRequest("Item is not for kitchen");

        if (!Enum.TryParse<OrderItemStatus>(req.Status, true, out var newStatus)) return BadRequest("Invalid status");

        // validate transition
        if (item.Status == OrderItemStatus.SENT && newStatus == OrderItemStatus.IN_PROGRESS) { item.Status = newStatus; }
        else if (item.Status == OrderItemStatus.IN_PROGRESS && newStatus == OrderItemStatus.DONE) { item.Status = newStatus; }
        else if (newStatus == OrderItemStatus.CANCELLED) { item.Status = newStatus; }
        else return BadRequest("Invalid status transition");

        await _ctx.SaveChangesAsync();
        await _hub.Clients.Group(item.BusinessId.ToString()).SendAsync("kitchen.queue.updated");
        return Ok(item);
    }
}
