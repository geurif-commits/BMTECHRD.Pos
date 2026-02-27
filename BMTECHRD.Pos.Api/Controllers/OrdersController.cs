using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Api.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderBatchService _orderBatchService;

    public OrdersController(IOrderBatchService orderBatchService)
    {
        _orderBatchService = orderBatchService;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateOrderBatchRequest req, CancellationToken ct)
    {
        var resp = await _orderBatchService.CreateBatchAsync(req, ct);
        return Ok(resp);
    }
}
