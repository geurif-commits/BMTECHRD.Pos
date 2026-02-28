using BMTECHRD.Pos.Api.Services.Tables;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/tables")]
public sealed class TablesController : ControllerBase
{
    private readonly ITablesService _tablesService;

    public TablesController(ITablesService tablesService)
    {
        _tablesService = tablesService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId, CancellationToken ct)
    {
        var list = await _tablesService.GetAsync(businessId, ct);
        return Ok(list);
    }

    [HttpPost("{id}/open")]
    public async Task<IActionResult> Open([FromRoute] Guid id, [FromBody] OpenTableRequest req, CancellationToken ct)
    {
        var table = await _tablesService.OpenAsync(id, req, ct);
        return Ok(table);
    }

    [HttpPost("{id}/access")]
    public async Task<IActionResult> Access([FromRoute] Guid id, [FromBody] TableAccessRequest req, CancellationToken ct)
    {
        var resp = await _tablesService.AccessAsync(id, req, ct);
        return Ok(resp);
    }

    [HttpPatch("{id}/position")]
    public async Task<IActionResult> UpdatePosition([FromRoute] Guid id, [FromBody] UpdateTablePositionRequest req, CancellationToken ct)
    {
        var table = await _tablesService.UpdatePositionAsync(id, req, ct);
        return Ok(table);
    }
}
