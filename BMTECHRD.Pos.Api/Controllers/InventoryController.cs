using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public InventoryController(AppDbContext ctx) => _ctx = ctx;

    public sealed class AdjustRequest
    {
        public required System.Guid BusinessId { get; set; }
        public required System.Guid ProductId { get; set; }
        public int QuantityDelta { get; set; }
        public required string Reason { get; set; }
        public System.Guid? ActorUserId { get; set; }
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustRequest req)
    {
        var product = await _ctx.Products.FindAsync(req.ProductId);
        if (product == null || product.BusinessId != req.BusinessId) return NotFound("Product not found for business");

        if (product.TrackInventory && product.Stock + req.QuantityDelta < 0)
        {
            return BadRequest("Insufficient stock");
        }

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

        return Ok(new { product.Id, product.Stock });
    }
}
