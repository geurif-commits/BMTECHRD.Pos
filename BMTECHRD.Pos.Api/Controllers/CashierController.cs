using BMTECHRD.Pos.Api.Services.Cashier;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/cash")]
public sealed class CashierController : ControllerBase
{
    private readonly ICashierService _cashierService;

    public CashierController(ICashierService cashierService)
    {
        _cashierService = cashierService;
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetOpenTables([FromQuery] Guid businessId, CancellationToken ct)
    {
        var res = await _cashierService.GetOpenTablesAsync(businessId, ct);
        return Ok(res);
    }

    [HttpGet("bill")]
    public async Task<IActionResult> GetBill([FromQuery] Guid businessId, [FromQuery] Guid tableId, CancellationToken ct)
    {
        var resp = await _cashierService.GetBillAsync(businessId, tableId, ct);
        return Ok(resp);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest req, CancellationToken ct)
    {
        Request.Headers.TryGetValue("Idempotency-Key", out var key);
        var idempotencyKey = key.FirstOrDefault();

        var resp = await _cashierService.CreatePaymentAsync(req, idempotencyKey, ct);
        return Ok(resp);
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseTable([FromBody] CloseTableRequest req, CancellationToken ct)
    {
        var closed = await _cashierService.CloseTableAsync(req, ct);
        return Ok(new { Closed = closed });
    }
}
