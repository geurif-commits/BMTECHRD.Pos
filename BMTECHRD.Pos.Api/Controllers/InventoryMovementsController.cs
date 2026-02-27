using BMTECHRD.Pos.Api.Services.InventoryMovements;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryMovementsController : ControllerBase
{
    private readonly IInventoryMovementsService _service;

    public InventoryMovementsController(IInventoryMovementsService service)
    {
        _service = service;
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid businessId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        var list = await _service.GetMovementsAsync(businessId, productId, from, to, limit, ct);
        return Ok(list);
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock([FromQuery] Guid businessId, CancellationToken ct)
    {
        var list = await _service.GetStockAsync(businessId, ct);
        return Ok(list);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] InventoryAdjustRequest req, CancellationToken ct)
    {
        var result = await _service.AdjustAsync(req, ct);
        return Ok(new { productId = result.ProductId, result.Stock });
    }
}
