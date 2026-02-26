using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/tables")]
public sealed class TablesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;
    private readonly ITableAccessPolicy _policy;

    public TablesController(AppDbContext ctx, IPasswordHasher hasher, ITableAccessPolicy policy)
    {
        _ctx = ctx;
        _hasher = hasher;
        _policy = policy;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId)
    {
        var list = await _ctx.Tables.Where(t => t.BusinessId == businessId).OrderBy(t => t.Number).ToListAsync();
        return Ok(list);
    }

    [HttpPost("{id}/open")]
    public async Task<IActionResult> Open([FromRoute] Guid id, [FromBody] OpenTableRequest req)
    {
        var table = await _ctx.Tables.FindAsync(id);
        if (table == null) return NotFound();
        if (table.Status != BMTECHRD.Pos.Domain.Enums.TableStatus.AVAILABLE) return BadRequest("Table not available");
        table.Status = BMTECHRD.Pos.Domain.Enums.TableStatus.OPEN;
        table.OpenedByWaiterId = req.WaiterId;
        table.OpenedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(table);
    }

    [HttpPost("{id}/access")]
    public async Task<IActionResult> Access([FromRoute] Guid id, [FromBody] TableAccessRequest req)
    {
        var table = await _ctx.Tables.FindAsync(id);
        if (table == null) return NotFound();
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null) return NotFound("Actor user not found");

        var requiresPin = _policy.RequiresPin(actor.Role, table.OpenedByWaiterId, req.ActorUserId);
        if (!requiresPin)
        {
            return Ok(new TableAccessResponse { RequiresPin = false, AccessGranted = true });
        }

        // validate PIN format
        if (string.IsNullOrWhiteSpace(req.Pin) || req.Pin.Length != 4 || !req.Pin.All(char.IsDigit))
        {
            return BadRequest(new TableAccessResponse { RequiresPin = true, AccessGranted = false, Reason = "Invalid PIN format" });
        }

        var ok = _hasher.Verify(req.Pin, actor.PinHash ?? string.Empty);
        if (!ok)
        {
            return Ok(new TableAccessResponse { RequiresPin = true, AccessGranted = false, Reason = "Invalid PIN" });
        }

        return Ok(new TableAccessResponse { RequiresPin = true, AccessGranted = true });
    }

    [HttpPatch("{id}/position")]
    public async Task<IActionResult> UpdatePosition([FromRoute] Guid id, [FromBody] UpdateTablePositionRequest req)
    {
        var table = await _ctx.Tables.FindAsync(id);
        if (table == null) return NotFound();
        table.PosX = req.PosX;
        table.PosY = req.PosY;
        await _ctx.SaveChangesAsync();
        return Ok(table);
    }
}
