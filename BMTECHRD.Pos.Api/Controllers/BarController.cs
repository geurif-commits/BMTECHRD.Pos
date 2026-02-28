using BMTECHRD.Pos.Api.Services.Production;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/bar")]
public sealed class BarController : ControllerBase
{
    private readonly IProductionQueueService _productionQueueService;

    public BarController(IProductionQueueService productionQueueService)
    {
        _productionQueueService = productionQueueService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] Guid businessId, CancellationToken ct)
    {
        var items = await _productionQueueService.GetQueueAsync(businessId, ProductionArea.BAR, ct);
        return Ok(items);
    }

    [HttpPatch("items/{orderItemId}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid orderItemId, [FromBody] UpdateOrderItemStatusRequest req, CancellationToken ct)
    {
        var item = await _productionQueueService.UpdateStatusAsync(orderItemId, req.Status, ProductionArea.BAR, ct);
        return Ok(item);
    }
}
