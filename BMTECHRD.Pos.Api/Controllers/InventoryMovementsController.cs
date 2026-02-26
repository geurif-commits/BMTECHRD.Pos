using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryMovementsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public InventoryMovementsController(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid businessId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? limit)
    {
        var take = limit ?? 200;
        if (take <= 0) take = 200;
        if (take > 1000) take = 1000;

        var query = _ctx.InventoryMovements
            .AsNoTracking()
            .Where(m => m.BusinessId == businessId);

        if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
        if (from.HasValue) query = query.Where(m => m.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(m => m.CreatedAt <= to.Value);

        var list = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product != null ? m.Product.Name : string.Empty,
                QuantityDelta = m.QuantityDelta,
                Reason = m.Reason,
                ActorUserId = m.ActorUserId,
                ActorUsername = m.ActorUserId.HasValue
                    ? _ctx.Users
                        .Where(u => u.Id == m.ActorUserId)
                        .Select(u => u.Username)
                        .FirstOrDefault()
                    : null,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock([FromQuery] Guid businessId)
    {
        var products = await _ctx.Products
            .AsNoTracking()
            .Where(p => p.BusinessId == businessId)
            .ToListAsync();

        var list = products.Select(p => new StockItemDto
        {
            ProductId = p.Id,
            Name = p.Name,
            Stock = p.Stock,
            TrackInventory = p.TrackInventory
        }).ToList();

        return Ok(list);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] InventoryAdjustRequest req)
    {
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null || actor.BusinessId != req.BusinessId) return Forbid();
        if (actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERVISOR) return Forbid();

        var product = await _ctx.Products
            .FirstOrDefaultAsync(p => p.Id == req.ProductId && p.BusinessId == req.BusinessId);

        if (product == null) return BadRequest("Product not found");
        if (req.QuantityDelta == 0) return BadRequest("QuantityDelta must be non-zero");

        if (product.TrackInventory && product.Stock + req.QuantityDelta < 0)
            return BadRequest("Insufficient stock for adjustment");

        product.Stock += req.QuantityDelta;

        var mov = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ProductId = req.ProductId,
            QuantityDelta = req.QuantityDelta,
            Reason = req.Reason,
            ActorUserId = req.ActorUserId,
            CreatedAt = DateTime.UtcNow
        };

        _ctx.InventoryMovements.Add(mov);
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("inventory.updated");
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated");

        return Ok(new { productId = product.Id, product.Stock });
    }
}
