using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/shifts")]
public sealed class ShiftsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public ShiftsController(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] Guid businessId, [FromQuery] Guid userId)
    {
        var shift = await _ctx.Shifts.FirstOrDefaultAsync(s => s.BusinessId == businessId && s.UserId == userId && s.Status == "OPEN");
        if (shift == null) return Ok(new ShiftStatusResponse { Status = "NONE" });

        var user = await _ctx.Users.FindAsync(shift.UserId);
        var resp = new ShiftStatusResponse
        {
            ShiftId = shift.Id,
            Status = "OPEN",
            OpenedAt = shift.OpenedAt,
            OpenedByUsername = user?.Username,
            OpeningCash = shift.OpeningCash
        };
        return Ok(resp);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] Guid businessId, [FromQuery] Guid? userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status, [FromQuery] int? limit, [FromQuery] Guid? actorUserId)
    {
        // optional role check: if actorUserId provided, ensure ADMIN/SUPERVISOR
        if (actorUserId.HasValue)
        {
            var actor = await _ctx.Users.FindAsync(actorUserId.Value);
            if (actor == null || actor.BusinessId != businessId) return Forbid();
            if (!(actor.Role == BMTECHRD.Pos.Domain.Enums.UserRole.ADMIN || actor.Role == BMTECHRD.Pos.Domain.Enums.UserRole.SUPERVISOR)) return Forbid();
        }
        var take = limit ?? 200;
        if (take <= 0) take = 200;
        if (take > 1000) take = 1000;

        var query = _ctx.Shifts.Where(s => s.BusinessId == businessId).AsQueryable();
        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);
        if (from.HasValue) query = query.Where(s => s.OpenedAt >= from.Value);
        if (to.HasValue) query = query.Where(s => s.OpenedAt <= to.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(s => s.Status == status);

        var list = await query.OrderByDescending(s => s.OpenedAt)
            .Take(take)
            .Select(s => new BMTECHRD.Pos.Application.DTOs.ShiftListItemDto
            {
                ShiftId = s.Id,
                UserId = s.UserId,
                Username = _ctx.Users.Where(u => u.Id == s.UserId).Select(u => u.Username).FirstOrDefault() ?? string.Empty,
                OpenedAt = s.OpenedAt,
                ClosedAt = s.ClosedAt,
                OpeningCash = s.OpeningCash,
                ClosingCash = s.ClosingCash,
                Status = s.Status
            }).ToListAsync();

        return Ok(list);
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] CreateShiftRequest req)
    {
        // role validation omitted
        // ensure this user does not have an open shift
        var existing = await _ctx.Shifts.AnyAsync(s => s.UserId == req.UserId && s.Status == "OPEN");
        if (existing) return Conflict("A shift is already open for this user");

        var shift = new BMTECHRD.Pos.Domain.Entities.Shift
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            UserId = req.UserId,
            OpenedAt = DateTime.UtcNow,
            OpeningCash = req.OpeningCash,
            Status = "OPEN",
            Notes = req.Notes
        };

        _ctx.Shifts.Add(shift);
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated");

        return Ok(new CreateShiftResponse { ShiftId = shift.Id, OpenedAt = shift.OpenedAt });
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseShiftRequest req)
    {
        var shift = await _ctx.Shifts.FindAsync(req.ShiftId);
        if (shift == null || shift.BusinessId != req.BusinessId || shift.Status != "OPEN") return BadRequest("Shift not found or not open");

        if (shift.UserId != req.UserId) return BadRequest("Shift does not belong to this user");

        // compute summary
        var payments = await _ctx.Payments.Where(p => p.ShiftId == req.ShiftId).ToListAsync();
        var salesCash = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CASH).Sum(p => p.Amount);
        var salesCard = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CARD).Sum(p => p.Amount);
        var salesTransfer = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.TRANSFER).Sum(p => p.Amount);
        var salesMixed = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.MIXED).Sum(p => p.Amount);
        var totalPayments = payments.Sum(p => p.Amount);

        shift.ClosingCash = req.ClosingCash;
        shift.ClosedAt = DateTime.UtcNow;
        shift.Status = "CLOSED";
        shift.Notes = req.Notes;
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated");

        var expectedCash = shift.OpeningCash + salesCash;
        var diff = (req.ClosingCash - expectedCash);

        var resp = new BMTECHRD.Pos.Application.DTOs.ShiftSummaryDto
        {
            ShiftId = shift.Id,
            UserId = shift.UserId,
            Username = _ctx.Users.Where(u => u.Id == shift.UserId).Select(u => u.Username).FirstOrDefault() ?? string.Empty,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            Status = shift.Status,
            OpeningCash = shift.OpeningCash,
            ClosingCash = shift.ClosingCash,
            SalesCash = salesCash,
            SalesCard = salesCard,
            SalesTransfer = salesTransfer,
            SalesMixed = salesMixed,
            TotalPayments = totalPayments,
            ExpectedCash = expectedCash,
            Difference = shift.Status == "CLOSED" ? diff : (decimal?)null
        };
        return Ok(resp);
    }

    [HttpGet("{id}/summary")]
    public async Task<IActionResult> Summary([FromRoute] Guid id, [FromQuery] Guid businessId)
    {
        var shift = await _ctx.Shifts.FindAsync(id);
        if (shift == null || shift.BusinessId != businessId) return NotFound();

        var payments = await _ctx.Payments.Where(p => p.ShiftId == id).ToListAsync();
        var salesCash = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CASH).Sum(p => p.Amount);
        var salesCard = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CARD).Sum(p => p.Amount);
        var salesTransfer = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.TRANSFER).Sum(p => p.Amount);
        var salesMixed = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.MIXED).Sum(p => p.Amount);
        var totalPayments = payments.Sum(p => p.Amount);

        var expectedCash = shift.OpeningCash + salesCash;
        var diff = (shift.ClosingCash ?? 0m) - expectedCash;

        var resp = new BMTECHRD.Pos.Application.DTOs.ShiftSummaryDto
        {
            ShiftId = shift.Id,
            UserId = shift.UserId,
            Username = _ctx.Users.Where(u => u.Id == shift.UserId).Select(u => u.Username).FirstOrDefault() ?? string.Empty,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            Status = shift.Status,
            OpeningCash = shift.OpeningCash,
            ClosingCash = shift.ClosingCash,
            SalesCash = salesCash,
            SalesCard = salesCard,
            SalesTransfer = salesTransfer,
            SalesMixed = salesMixed,
            TotalPayments = totalPayments,
            ExpectedCash = expectedCash,
            Difference = shift.Status == "CLOSED" ? diff : (decimal?)null
        };
        return Ok(resp);
    }
}
