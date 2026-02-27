using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "SupervisorOrAdmin")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("sales/daily")]
    public async Task<IActionResult> SalesDaily([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (!from.HasValue || !to.HasValue)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid date range", "from and to are required", "REPORT_RANGE_REQUIRED");

        var businessId = User.GetRequiredBusinessId();
        var userId = User.GetRequiredUserId();

        var resp = await _reportsService.GetSalesDailyAsync(businessId, userId, from.Value, to.Value, ct);
        return Ok(resp);
    }

    [HttpGet("sales/by-product")]
    public async Task<IActionResult> SalesByProduct([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? limit, CancellationToken ct)
    {
        if (!from.HasValue || !to.HasValue)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid date range", "from and to are required", "REPORT_RANGE_REQUIRED");

        var businessId = User.GetRequiredBusinessId();
        var userId = User.GetRequiredUserId();

        var resp = await _reportsService.GetSalesByProductAsync(businessId, userId, from.Value, to.Value, limit, ct);
        return Ok(resp);
    }

    [HttpGet("sales/by-user")]
    public async Task<IActionResult> SalesByUser([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (!from.HasValue || !to.HasValue)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid date range", "from and to are required", "REPORT_RANGE_REQUIRED");

        var businessId = User.GetRequiredBusinessId();
        var userId = User.GetRequiredUserId();

        var resp = await _reportsService.GetSalesByUserAsync(businessId, userId, from.Value, to.Value, ct);
        return Ok(resp);
    }
}
