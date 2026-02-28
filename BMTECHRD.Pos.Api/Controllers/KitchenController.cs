using BMTECHRD.Pos.Api.Services.Production;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/kitchen")]
public sealed class KitchenController : ControllerBase
{
    private readonly IProductionQueueService _productionQueueService;

    public KitchenController(IProductionQueueService productionQueueService)
    {
        _productionQueueService = productionQueueService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] Guid businessId, CancellationToken ct)
    {
        var items = await _productionQueueService.GetQueueAsync(businessId, ProductionArea.KITCHEN, ct);
        return Ok(items);
    }

    [HttpPatch("items/{orderItemId}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid orderItemId, [FromBody] UpdateOrderItemStatusRequest req, CancellationToken ct)
    {
        var item = await _productionQueueService.UpdateStatusAsync(orderItemId, req.Status, ProductionArea.KITCHEN, ct);
        return Ok(item);
    }
}
