using System.Text.Json;
using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Cashier;

public sealed class CashierService : ICashierService
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    private const string IdempotencyScope = "CASHIER_PAYMENT_CREATE";

    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;
    private readonly IIdempotencyKeyStore _idempotencyKeyStore;

    public CashierService(AppDbContext ctx, IHubContext<PosHub> hub, IIdempotencyKeyStore idempotencyKeyStore)
    {
        _ctx = ctx;
        _hub = hub;
        _idempotencyKeyStore = idempotencyKeyStore;
    }

    public async Task<List<TableSummaryDto>> GetOpenTablesAsync(Guid businessId, CancellationToken ct)
    {
        var tables = await _ctx.Tables
            .AsNoTracking()
            .Where(t => t.BusinessId == businessId && t.Status == TableStatus.OPEN)
            .ToListAsync(ct);

        var result = new List<TableSummaryDto>(tables.Count);

        foreach (var t in tables)
        {
            var orderIds = await _ctx.Orders.AsNoTracking().Where(o => o.TableId == t.Id).Select(o => o.Id).ToListAsync(ct);
            var relevant = await _ctx.OrderItems.AsNoTracking()
                .Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != OrderItemStatus.CANCELLED)
                .ToListAsync(ct);

            var total = relevant.Sum(r => r.UnitPriceSnapshot * r.Quantity);
            var count = relevant.Sum(r => r.Quantity);
            var hasPending = relevant.Any(oi => oi.Status == OrderItemStatus.SENT || oi.Status == OrderItemStatus.IN_PROGRESS);

            result.Add(new TableSummaryDto
            {
                TableId = t.Id,
                TableNumber = t.Number,
                Status = t.Status.ToString(),
                CurrentTotal = total,
                ItemsCount = count,
                HasPendingItems = hasPending
            });
        }

        return result;
    }

    public async Task<GetBillResponse> GetBillAsync(Guid businessId, Guid tableId, CancellationToken ct)
    {
        var table = await _ctx.Tables.FindAsync(new object?[] { tableId }, ct);
        if (table == null || table.BusinessId != businessId)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Table not found", "Table not found", "CASH_TABLE_NOT_FOUND");

        var orders = await _ctx.Orders.AsNoTracking().Where(o => o.TableId == tableId).ToListAsync(ct);
        var orderIds = orders.Select(o => o.Id).ToList();

        var items = await _ctx.OrderItems.AsNoTracking()
            .Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != OrderItemStatus.CANCELLED)
            .ToListAsync(ct);

        var lines = items.GroupBy(i => new { i.ProductId, i.UnitPriceSnapshot, i.ProductNameSnapshot, i.Area })
            .Select(g => new BillLineDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductNameSnapshot,
                Quantity = g.Sum(x => x.Quantity),
                UnitPrice = g.Key.UnitPriceSnapshot,
                LineTotal = g.Sum(x => x.UnitPriceSnapshot * x.Quantity),
                Area = g.Key.Area.ToString(),
                Status = g.Any(x => x.Status == OrderItemStatus.SENT) ? "SENT" : (g.Any(x => x.Status == OrderItemStatus.IN_PROGRESS) ? "IN_PROGRESS" : "DONE")
            }).ToList();

        var subtotal = lines.Sum(l => l.LineTotal);
        var total = subtotal;

        var paid = await _ctx.Payments.AsNoTracking().Where(p => p.TableId == tableId && p.BusinessId == businessId).SumAsync(p => p.Amount, ct);
        var due = total - paid;

        var hasPending = items.Any(oi => oi.Status == OrderItemStatus.SENT || oi.Status == OrderItemStatus.IN_PROGRESS);

        return new GetBillResponse
        {
            BusinessId = businessId,
            TableId = tableId,
            TableNumber = table.Number,
            Lines = lines,
            Subtotal = subtotal,
            Tax = 0m,
            Tip = 0m,
            Discount = 0m,
            Total = total,
            Paid = paid,
            Due = due,
            HasPendingItems = hasPending
        };
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest req, string? idempotencyKey, CancellationToken ct)
    {
        if (req.Amount <= 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid amount", "Amount must be positive", "CASH_AMOUNT_INVALID");

        var table = await _ctx.Tables.FindAsync(new object?[] { req.TableId }, ct);
        if (table == null || table.BusinessId != req.BusinessId)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Table invalid", "Table not found for business", "CASH_TABLE_INVALID");

        if (table.Status != TableStatus.OPEN)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Table invalid", "Table is not open", "CASH_TABLE_NOT_OPEN");

        var shift = await _ctx.Shifts.FindAsync(new object?[] { req.ShiftId }, ct);
        if (shift == null || shift.BusinessId != req.BusinessId || shift.Status != "OPEN")
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Shift invalid", "Invalid or closed shift", "CASH_SHIFT_INVALID");

        if (shift.UserId != req.ActorUserId)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Shift invalid", "Shift does not belong to actor user", "CASH_SHIFT_USER_MISMATCH");

        if (!Enum.TryParse<PaymentMethod>(req.Method, true, out var method))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Payment method invalid", "Invalid payment method", "CASH_METHOD_INVALID");

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var replayPaymentId = await _idempotencyKeyStore.TryGetEntityIdAsync(IdempotencyScope, req.BusinessId, req.ActorUserId, idempotencyKey, ct);
            if (replayPaymentId.HasValue)
            {
                var existingPayment = await _ctx.Payments.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == replayPaymentId.Value, ct);

                if (existingPayment != null)
                {
                    return await BuildPaymentStateAsync(req.BusinessId, req.TableId, existingPayment.Amount, req.CashGiven, closed: false, closeBlockedReason: "Idempotent replay", ct);
                }
            }
        }

        var dueBefore = await ComputeDueAsync(req.BusinessId, req.TableId, ct);
        if (method != PaymentMethod.CASH && req.Amount > dueBefore)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Amount invalid", "Non-cash payments cannot exceed due amount", "CASH_NON_CASH_EXCEEDS_DUE");

        var payment = new BMTECHRD.Pos.Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            TableId = req.TableId,
            ShiftId = req.ShiftId,
            CreatedByUserId = req.ActorUserId,
            Method = method,
            Amount = req.Amount,
            MetaJson = JsonSerializer.Serialize(new
            {
                requestMeta = req.Meta,
                idempotencyKey
            }),
            CreatedAt = DateTime.UtcNow
        };

        _ctx.Payments.Add(payment);

        await _ctx.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _idempotencyKeyStore.SaveAsync(IdempotencyScope, req.BusinessId, req.ActorUserId, idempotencyKey, payment.Id, IdempotencyTtl, ct);
        }

        var dueAfter = await ComputeDueAsync(req.BusinessId, req.TableId, ct);

        var closed = false;
        string? closeBlockedReason = null;
        if (req.CloseIfPaid && dueAfter <= 0)
        {
            var orderIds = await _ctx.Orders.AsNoTracking().Where(o => o.TableId == req.TableId).Select(o => o.Id).ToListAsync(ct);
            var hasPending = await _ctx.OrderItems.AsNoTracking().AnyAsync(
                oi => orderIds.Contains(oi.OrderId) && (oi.Status == OrderItemStatus.SENT || oi.Status == OrderItemStatus.IN_PROGRESS), ct);

            if (hasPending)
            {
                closeBlockedReason = "Hay pedidos pendientes (Pendiente/En proceso). No se puede cerrar la factura.";
            }
            else
            {
                table.Status = TableStatus.AVAILABLE;
                table.OpenedAt = null;
                table.OpenedByWaiterId = null;
                table.UpdatedAt = DateTime.UtcNow;
                await _ctx.SaveChangesAsync(ct);
                closed = true;
            }
        }

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated", cancellationToken: ct);

        return await BuildPaymentStateAsync(req.BusinessId, req.TableId, req.Amount, req.CashGiven, closed, closeBlockedReason, ct);
    }

    public async Task<bool> CloseTableAsync(CloseTableRequest req, CancellationToken ct)
    {
        var table = await _ctx.Tables.FindAsync(new object?[] { req.TableId }, ct);
        if (table == null || table.BusinessId != req.BusinessId)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Table invalid", "Table not found", "CASH_TABLE_NOT_FOUND");

        var due = await ComputeDueAsync(req.BusinessId, req.TableId, ct);
        if (due > 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Table cannot close", "Due amount must be paid before closing table", "CASH_DUE_PENDING");

        table.Status = TableStatus.AVAILABLE;
        table.OpenedAt = null;
        table.OpenedByWaiterId = null;
        table.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated", cancellationToken: ct);
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated", cancellationToken: ct);

        return true;
    }

    private async Task<decimal> ComputeDueAsync(Guid businessId, Guid tableId, CancellationToken ct)
    {
        var paid = await _ctx.Payments.AsNoTracking().Where(p => p.TableId == tableId && p.BusinessId == businessId).SumAsync(p => p.Amount, ct);
        var orderIds = await _ctx.Orders.AsNoTracking().Where(o => o.TableId == tableId).Select(o => o.Id).ToListAsync(ct);
        var subtotal = await _ctx.OrderItems.AsNoTracking()
            .Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != OrderItemStatus.CANCELLED)
            .SumAsync(i => i.UnitPriceSnapshot * i.Quantity, ct);

        return subtotal - paid;
    }

    private async Task<CreatePaymentResponse> BuildPaymentStateAsync(Guid businessId, Guid tableId, decimal amount, decimal? cashGiven, bool closed, string? closeBlockedReason, CancellationToken ct)
    {
        var due = await ComputeDueAsync(businessId, tableId, ct);
        var paid = await _ctx.Payments.AsNoTracking().Where(p => p.TableId == tableId && p.BusinessId == businessId).SumAsync(p => p.Amount, ct);
        var change = 0m;
        if (cashGiven.HasValue && cashGiven.Value > amount)
            change = cashGiven.Value - amount;

        return new CreatePaymentResponse
        {
            Paid = paid,
            Due = due,
            Change = change,
            Closed = closed,
            CloseBlockedReason = closeBlockedReason
        };
    }
}
