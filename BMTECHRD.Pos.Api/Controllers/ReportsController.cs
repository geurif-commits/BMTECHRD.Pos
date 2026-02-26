using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "SupervisorOrAdmin")]
public sealed class ReportsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public ReportsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Obtiene reporte diario de ventas.
    /// Requiere autenticación con rol ADMIN o SUPERVISOR.
    /// </summary>
    [HttpGet("sales/daily")]
    public async Task<IActionResult> SalesDaily([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        // Extraer claims JWT
        if (!Guid.TryParse(User.FindFirst("bid")?.Value, out var businessId))
            return Unauthorized("Missing or invalid businessId claim");

        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
            return Unauthorized("Missing or invalid user ID claim");

        // Validar parámetros
        if (!from.HasValue || !to.HasValue) 
            return BadRequest("from and to are required");
        if (from > to) 
            return BadRequest("from must be <= to");

        var start = from.Value.ToUniversalTime();
        var end = to.Value.ToUniversalTime();

        // daily totals from OrderItems where Status == DONE
        var daily = await _ctx.OrderItems
            .AsNoTracking()
            .Where(oi => oi.BusinessId == businessId && oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.DONE && oi.CreatedAt >= start && oi.CreatedAt <= end)
            .GroupBy(oi => new { oi.CreatedAt.Year, oi.CreatedAt.Month, oi.CreatedAt.Day })
            .Select(g => new SalesDailyDto
            {
                Date = new System.DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                Total = g.Sum(x => x.UnitPriceSnapshot * x.Quantity)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        // payments summary within range
        var payments = await _ctx.Payments.AsNoTracking().Where(p => p.BusinessId == businessId && p.CreatedAt >= start && p.CreatedAt <= end).ToListAsync();
        var paymentsSummary = new PaymentsSummaryDto
        {
            Cash = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CASH).Sum(p => p.Amount),
            Card = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.CARD).Sum(p => p.Amount),
            Transfer = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.TRANSFER).Sum(p => p.Amount),
            Mixed = payments.Where(p => p.Method == BMTECHRD.Pos.Domain.Enums.PaymentMethod.MIXED).Sum(p => p.Amount)
        };
        paymentsSummary.Total = paymentsSummary.Cash + paymentsSummary.Card + paymentsSummary.Transfer + paymentsSummary.Mixed;

        // Registrar acceso en auditoría
        var auditLog = new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_DAILY",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O}",
            CreatedAt = DateTime.UtcNow
        };
        _ctx.AuditLogs.Add(auditLog);
        await _ctx.SaveChangesAsync();

        var resp = new DailySalesReportResponse { Items = daily, Payments = paymentsSummary };
        return Ok(resp);
    }

    /// <summary>
    /// Obtiene reporte de ventas por producto.
    /// Requiere autenticación con rol ADMIN o SUPERVISOR.
    /// </summary>
    [HttpGet("sales/by-product")]
    public async Task<IActionResult> SalesByProduct([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? limit)
    {
        // Extraer claims JWT
        if (!Guid.TryParse(User.FindFirst("bid")?.Value, out var businessId))
            return Unauthorized("Missing or invalid businessId claim");

        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
            return Unauthorized("Missing or invalid user ID claim");

        // Validar parámetros
        if (!from.HasValue || !to.HasValue) 
            return BadRequest("from and to are required");
        if (from > to) 
            return BadRequest("from must be <= to");

        var take = limit ?? 50;
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var start = from.Value.ToUniversalTime();
        var end = to.Value.ToUniversalTime();

        var list = await _ctx.OrderItems
            .AsNoTracking()
            .Where(oi => oi.BusinessId == businessId && oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.DONE && oi.CreatedAt >= start && oi.CreatedAt <= end)
            .GroupBy(oi => new { oi.ProductId, oi.ProductNameSnapshot })
            .Select(g => new SalesByProductDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductNameSnapshot,
                Qty = g.Sum(x => x.Quantity),
                Total = g.Sum(x => x.UnitPriceSnapshot * x.Quantity)
            })
            .OrderByDescending(x => x.Total)
            .Take(take)
            .ToListAsync();

        // Registrar acceso en auditoría
        var auditLog = new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_BY_PRODUCT",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O},limit={take}",
            CreatedAt = DateTime.UtcNow
        };
        _ctx.AuditLogs.Add(auditLog);
        await _ctx.SaveChangesAsync();

        return Ok(list);
    }

    /// <summary>
    /// Obtiene reporte de ventas por usuario.
    /// Requiere autenticación con rol ADMIN o SUPERVISOR.
    /// </summary>
    [HttpGet("sales/by-user")]
    public async Task<IActionResult> SalesByUser([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        // Extraer claims JWT
        if (!Guid.TryParse(User.FindFirst("bid")?.Value, out var businessId))
            return Unauthorized("Missing or invalid businessId claim");

        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
            return Unauthorized("Missing or invalid user ID claim");

        // Validar parámetros
        if (!from.HasValue || !to.HasValue) 
            return BadRequest("from and to are required");
        if (from > to) 
            return BadRequest("from must be <= to");

        var start = from.Value.ToUniversalTime();
        var end = to.Value.ToUniversalTime();

        // join payments -> shifts to get shift.UserId
        var query = from p in _ctx.Payments.AsNoTracking()
                    join s in _ctx.Shifts.AsNoTracking() on p.ShiftId equals s.Id
                    where p.BusinessId == businessId && p.CreatedAt >= start && p.CreatedAt <= end
                    select new { s.UserId, p.Amount };

        var grouped = await query.GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var result = grouped.Select(g => new SalesByUserDto
        {
            UserId = g.UserId,
            Username = _ctx.Users.Where(u => u.Id == g.UserId).Select(u => u.Username).FirstOrDefault() ?? string.Empty,
            Total = g.Total
        }).ToList();

        // Registrar acceso en auditoría
        var auditLog = new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_BY_USER",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O}",
            CreatedAt = DateTime.UtcNow
        };
        _ctx.AuditLogs.Add(auditLog);
        await _ctx.SaveChangesAsync();

        return Ok(result);
    }
}
