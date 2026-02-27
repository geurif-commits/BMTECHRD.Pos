using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Production;

public sealed class ProductionQueueService : IProductionQueueService
{
    private const string TopicKitchenQueueUpdated = "kitchen.queue.updated";
    private const string TopicBarQueueUpdated = "bar.queue.updated";

    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public ProductionQueueService(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    public async Task<List<ProductionQueueItemDto>> GetQueueAsync(Guid businessId, ProductionArea area, CancellationToken ct)
    {
        return await _ctx.OrderItems.AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.Area == area && i.Status != OrderItemStatus.CANCELLED && i.Status != OrderItemStatus.DONE)
            .OrderBy(i => i.CreatedAt)
            .Join(_ctx.Orders.AsNoTracking(), oi => oi.OrderId, o => o.Id, (oi, o) => new { oi, o })
            .Join(_ctx.Tables.AsNoTracking(), x => x.o.TableId, t => t.Id, (x, t) => new ProductionQueueItemDto
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
            .ToListAsync(ct);
    }

    public async Task<OrderItem> UpdateStatusAsync(Guid orderItemId, string status, ProductionArea area, CancellationToken ct)
    {
        var item = await _ctx.OrderItems.FindAsync(new object?[] { orderItemId }, ct);
        if (item == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Order item not found", "Order item not found", "PROD_ITEM_NOT_FOUND");

        if (item.Area != area)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid area", $"Item is not for {area}", "PROD_ITEM_AREA_INVALID");

        if (!Enum.TryParse<OrderItemStatus>(status, true, out var newStatus))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid status", "Invalid status", "PROD_STATUS_INVALID");

        var validTransition =
            (item.Status == OrderItemStatus.SENT && newStatus == OrderItemStatus.IN_PROGRESS)
            || (item.Status == OrderItemStatus.IN_PROGRESS && newStatus == OrderItemStatus.DONE)
            || newStatus == OrderItemStatus.CANCELLED;

        if (!validTransition)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid status transition", "Invalid status transition", "PROD_STATUS_TRANSITION_INVALID");

        item.Status = newStatus;

        await _ctx.SaveChangesAsync(ct);

        var topic = area == ProductionArea.KITCHEN ? TopicKitchenQueueUpdated : TopicBarQueueUpdated;
        await _hub.Clients.Group(item.BusinessId.ToString()).SendAsync(topic, cancellationToken: ct);

        return item;
    }
}
