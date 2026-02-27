using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace BMTECHRD.Pos.Api.Services.Reports;

public sealed class ReportsService : IReportsService
{
    private readonly AppDbContext _ctx;

    public ReportsService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<DailySalesReportResponse> GetSalesDailyAsync(Guid businessId, Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        ValidateRange(from, to);

        var start = from.ToUniversalTime();
        var end = to.ToUniversalTime();

        var daily = await _ctx.OrderItems
            .AsNoTracking()
            .Where(oi => oi.BusinessId == businessId && oi.Status == OrderItemStatus.DONE && oi.CreatedAt >= start && oi.CreatedAt <= end)
            .GroupBy(oi => new { oi.CreatedAt.Year, oi.CreatedAt.Month, oi.CreatedAt.Day })
            .Select(g => new SalesDailyDto
            {
                Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                Total = g.Sum(x => x.UnitPriceSnapshot * x.Quantity)
            })
            .OrderBy(d => d.Date)
            .ToListAsync(ct);

        var payments = await _ctx.Payments.AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync(ct);

        var paymentsSummary = new PaymentsSummaryDto
        {
            Cash = payments.Where(p => p.Method == PaymentMethod.CASH).Sum(p => p.Amount),
            Card = payments.Where(p => p.Method == PaymentMethod.CARD).Sum(p => p.Amount),
            Transfer = payments.Where(p => p.Method == PaymentMethod.TRANSFER).Sum(p => p.Amount),
            Mixed = payments.Where(p => p.Method == PaymentMethod.MIXED).Sum(p => p.Amount)
        };
        paymentsSummary.Total = paymentsSummary.Cash + paymentsSummary.Card + paymentsSummary.Transfer + paymentsSummary.Mixed;

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_DAILY",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O}",
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync(ct);

        return new DailySalesReportResponse { Items = daily, Payments = paymentsSummary };
    }

    public async Task<List<SalesByProductDto>> GetSalesByProductAsync(Guid businessId, Guid userId, DateTime from, DateTime to, int? limit, CancellationToken ct)
    {
        ValidateRange(from, to);

        var take = limit ?? 50;
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var start = from.ToUniversalTime();
        var end = to.ToUniversalTime();

        var list = await _ctx.OrderItems
            .AsNoTracking()
            .Where(oi => oi.BusinessId == businessId && oi.Status == OrderItemStatus.DONE && oi.CreatedAt >= start && oi.CreatedAt <= end)
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
            .ToListAsync(ct);

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_BY_PRODUCT",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O},limit={take}",
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync(ct);

        return list;
    }

    public async Task<List<SalesByUserDto>> GetSalesByUserAsync(Guid businessId, Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        ValidateRange(from, to);

        var start = from.ToUniversalTime();
        var end = to.ToUniversalTime();

        var query = from p in _ctx.Payments.AsNoTracking()
                    join s in _ctx.Shifts.AsNoTracking() on p.ShiftId equals s.Id
                    where p.BusinessId == businessId && p.CreatedAt >= start && p.CreatedAt <= end
                    select new { s.UserId, p.Amount };

        var grouped = await query.GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync(ct);

        var userIds = grouped.Select(g => g.UserId).ToList();
        var users = await _ctx.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

        var result = grouped.Select(g => new SalesByUserDto
        {
            UserId = g.UserId,
            Username = users.TryGetValue(g.UserId, out var username) ? username : string.Empty,
            Total = g.Total
        }).ToList();

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = userId,
            Action = "REPORT_SALES_BY_USER",
            EntityType = "Report",
            DataJson = $"from={from:O},to={to:O}",
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync(ct);

        return result;
    }

    private static void ValidateRange(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid date range", "from must be <= to", "REPORT_INVALID_RANGE");
    }
}
