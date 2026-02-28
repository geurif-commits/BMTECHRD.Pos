using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Shifts;

public sealed class ShiftService : IShiftService
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    private const string OpenScope = "SHIFT_OPEN";
    private const string CloseScope = "SHIFT_CLOSE";
    private const string ShiftStatusOpen = "OPEN";
    private const string ShiftStatusNone = "NONE";

    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;
    private readonly IIdempotencyKeyStore _idempotencyKeyStore;

    public ShiftService(AppDbContext ctx, IHubContext<PosHub> hub, IIdempotencyKeyStore idempotencyKeyStore)
    {
        _ctx = ctx;
        _hub = hub;
        _idempotencyKeyStore = idempotencyKeyStore;
    }

    public async Task<ShiftStatusResponse> GetActiveAsync(Guid businessId, Guid userId, CancellationToken ct)
    {
        var shift = await _ctx.Shifts.AsNoTracking().FirstOrDefaultAsync(s => s.BusinessId == businessId && s.UserId == userId && s.Status == ShiftStatusOpen, ct);
        if (shift == null)
            return new ShiftStatusResponse { Status = ShiftStatusNone };

        var username = await _ctx.Users.AsNoTracking()
            .Where(u => u.Id == shift.UserId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(ct);

        return new ShiftStatusResponse
        {
            ShiftId = shift.Id,
            Status = ShiftStatusOpen,
            OpenedAt = shift.OpenedAt,
            OpenedByUsername = username,
            OpeningCash = shift.OpeningCash,
            SalesCash = 0m,
            SalesCard = 0m,
            SalesTransfer = 0m,
            SalesMixed = 0m,
            TotalPayments = 0m
        };
    }

    public async Task<List<ShiftListItemDto>> ListAsync(Guid businessId, Guid? userId, DateTime? from, DateTime? to, string? status, int? limit, Guid? actorUserId, CancellationToken ct)
    {
        if (actorUserId.HasValue)
        {
            var actor = await _ctx.Users.FindAsync(new object?[] { actorUserId.Value }, ct);
            if (actor == null || actor.BusinessId != businessId)
                throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor not allowed", "SHIFT_ACTOR_FORBIDDEN");
            if (actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERVISOR)
                throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor role not allowed", "SHIFT_ACTOR_ROLE_FORBIDDEN");
        }

        var take = limit ?? 100;
        if (take <= 0) take = 100;
        if (take > 1000) take = 1000;

        var query = _ctx.Shifts.AsNoTracking().Where(s => s.BusinessId == businessId);
        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);
        if (from.HasValue) query = query.Where(s => s.OpenedAt >= from.Value);
        if (to.HasValue) query = query.Where(s => s.OpenedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.Status == status);

        var rows = await query.OrderByDescending(s => s.OpenedAt).Take(take).ToListAsync(ct);
        var userIds = rows.Select(r => r.UserId).Distinct().ToList();
        var users = await _ctx.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username, ct);

        return rows.Select(s => new ShiftListItemDto
        {
            ShiftId = s.Id,
            UserId = s.UserId,
            Username = users.TryGetValue(s.UserId, out var uname) ? uname : string.Empty,
            OpenedAt = s.OpenedAt,
            ClosedAt = s.ClosedAt,
            OpeningCash = s.OpeningCash,
            ClosingCash = s.ClosingCash,
            Status = s.Status
        }).ToList();
    }

    public async Task<CreateShiftResponse> OpenAsync(CreateShiftRequest req, string? idempotencyKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var replayShiftId = await _idempotencyKeyStore.TryGetEntityIdAsync(OpenScope, req.BusinessId, req.UserId, idempotencyKey, ct);
            if (replayShiftId.HasValue)
            {
                var previous = await _ctx.Shifts.AsNoTracking().FirstOrDefaultAsync(s => s.Id == replayShiftId.Value, ct);
                if (previous != null)
                    return new CreateShiftResponse { ShiftId = previous.Id, OpenedAt = previous.OpenedAt };
            }
        }

        var existing = await _ctx.Shifts.AnyAsync(s => s.UserId == req.UserId && s.Status == ShiftStatusOpen, ct);
        if (existing)
            throw new ApiProblemException(StatusCodes.Status409Conflict, "Shift conflict", "A shift is already open for this user", "SHIFT_ALREADY_OPEN");

        var shift = new BMTECHRD.Pos.Domain.Entities.Shift
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            UserId = req.UserId,
            OpenedAt = DateTime.UtcNow,
            OpeningCash = req.OpeningCash,
            Status = ShiftStatusOpen,
            Notes = req.Notes
        };

        _ctx.Shifts.Add(shift);
        _ctx.AuditLogs.Add(new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ActorUserId = req.UserId,
            Action = "SHIFT_OPEN",
            EntityType = "Shift",
            EntityId = shift.Id,
            DataJson = "created",
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _idempotencyKeyStore.SaveAsync(OpenScope, req.BusinessId, req.UserId, idempotencyKey, shift.Id, IdempotencyTtl, ct);
        }

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated", cancellationToken: ct);

        return new CreateShiftResponse { ShiftId = shift.Id, OpenedAt = shift.OpenedAt };
    }

    public async Task<ShiftSummaryDto> CloseAsync(CloseShiftRequest req, string? idempotencyKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var replayShiftId = await _idempotencyKeyStore.TryGetEntityIdAsync(CloseScope, req.BusinessId, req.UserId, idempotencyKey, ct);
            if (replayShiftId.HasValue)
            {
                return await SummaryAsync(replayShiftId.Value, req.BusinessId, ct);
            }
        }

        var shift = await _ctx.Shifts.FindAsync(new object?[] { req.ShiftId }, ct);
        if (shift == null || shift.BusinessId != req.BusinessId || shift.Status != ShiftStatusOpen)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Shift invalid", "Shift not found or not open", "SHIFT_NOT_OPEN");

        if (shift.UserId != req.UserId)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Shift invalid", "Shift does not belong to this user", "SHIFT_USER_MISMATCH");

        var payments = await _ctx.Payments.AsNoTracking().Where(p => p.ShiftId == req.ShiftId).ToListAsync(ct);
        var salesCash = payments.Where(p => p.Method == PaymentMethod.CASH).Sum(p => p.Amount);
        var salesCard = payments.Where(p => p.Method == PaymentMethod.CARD).Sum(p => p.Amount);
        var salesTransfer = payments.Where(p => p.Method == PaymentMethod.TRANSFER).Sum(p => p.Amount);
        var salesMixed = payments.Where(p => p.Method == PaymentMethod.MIXED).Sum(p => p.Amount);
        var totalPayments = payments.Sum(p => p.Amount);

        shift.ClosingCash = req.ClosingCash;
        shift.ClosedAt = DateTime.UtcNow;
        shift.Status = "CLOSED";
        shift.Notes = req.Notes;
        _ctx.AuditLogs.Add(new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ActorUserId = req.UserId,
            Action = "SHIFT_CLOSE",
            EntityType = "Shift",
            EntityId = shift.Id,
            DataJson = "closed",
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _idempotencyKeyStore.SaveAsync(CloseScope, req.BusinessId, req.UserId, idempotencyKey, shift.Id, IdempotencyTtl, ct);
        }

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated", cancellationToken: ct);

        return await BuildSummaryAsync(shift, salesCash, salesCard, salesTransfer, salesMixed, totalPayments, ct);
    }

    public async Task<ShiftSummaryDto> SummaryAsync(Guid shiftId, Guid businessId, CancellationToken ct)
    {
        var shift = await _ctx.Shifts.FindAsync(new object?[] { shiftId }, ct);
        if (shift == null || shift.BusinessId != businessId)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Shift not found", "Shift not found", "SHIFT_NOT_FOUND");

        var payments = await _ctx.Payments.AsNoTracking().Where(p => p.ShiftId == shiftId).ToListAsync(ct);
        var salesCash = payments.Where(p => p.Method == PaymentMethod.CASH).Sum(p => p.Amount);
        var salesCard = payments.Where(p => p.Method == PaymentMethod.CARD).Sum(p => p.Amount);
        var salesTransfer = payments.Where(p => p.Method == PaymentMethod.TRANSFER).Sum(p => p.Amount);
        var salesMixed = payments.Where(p => p.Method == PaymentMethod.MIXED).Sum(p => p.Amount);
        var totalPayments = payments.Sum(p => p.Amount);

        return await BuildSummaryAsync(shift, salesCash, salesCard, salesTransfer, salesMixed, totalPayments, ct);
    }

    private async Task<ShiftSummaryDto> BuildSummaryAsync(BMTECHRD.Pos.Domain.Entities.Shift shift, decimal salesCash, decimal salesCard, decimal salesTransfer, decimal salesMixed, decimal totalPayments, CancellationToken ct)
    {
        var expectedCash = shift.OpeningCash + salesCash;
        var closing = shift.ClosingCash ?? 0m;
        var diff = closing - expectedCash;
        var username = await _ctx.Users.Where(u => u.Id == shift.UserId).Select(u => u.Username).FirstOrDefaultAsync(ct) ?? string.Empty;

        return new ShiftSummaryDto
        {
            ShiftId = shift.Id,
            UserId = shift.UserId,
            Username = username,
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
    }
}
