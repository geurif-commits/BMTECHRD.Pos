using BMTECHRD.Pos.Api.Services.Shifts;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/shifts")]
public sealed class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] Guid businessId, [FromQuery] Guid userId, CancellationToken ct)
    {
        var resp = await _shiftService.GetActiveAsync(businessId, userId, ct);
        return Ok(resp);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] Guid businessId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] int? limit,
        [FromQuery] Guid? actorUserId,
        CancellationToken ct)
    {
        var list = await _shiftService.ListAsync(businessId, userId, from, to, status, limit, actorUserId, ct);
        return Ok(list);
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] CreateShiftRequest req, CancellationToken ct)
    {
        var resp = await _shiftService.OpenAsync(req, ct);
        return Ok(resp);
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseShiftRequest req, CancellationToken ct)
    {
        var resp = await _shiftService.CloseAsync(req, ct);
        return Ok(resp);
    }

    [HttpGet("{id}/summary")]
    public async Task<IActionResult> Summary([FromRoute] Guid id, [FromQuery] Guid businessId, CancellationToken ct)
    {
        var resp = await _shiftService.SummaryAsync(id, businessId, ct);
        return Ok(resp);
    }
}
