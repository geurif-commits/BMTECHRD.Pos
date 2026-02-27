using BMTECHRD.Pos.Api.Services.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public sealed class AdjustRequest
    {
        public required Guid BusinessId { get; set; }
        public required Guid ProductId { get; set; }
        public int QuantityDelta { get; set; }
        public required string Reason { get; set; }
        public Guid? ActorUserId { get; set; }
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustRequest req, CancellationToken ct)
    {
        var result = await _inventoryService.AdjustAsync(req.BusinessId, req.ProductId, req.QuantityDelta, req.Reason, req.ActorUserId, ct);
        return Ok(new { Id = result.ProductId, result.Stock });
    }
}
