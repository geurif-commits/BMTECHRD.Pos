using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/cash")]
public sealed class CashierController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<PosHub> _hub;

    public CashierController(AppDbContext ctx, IHubContext<PosHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetOpenTables([FromQuery] Guid businessId)
    {
        var tables = await _ctx.Tables.Where(t => t.BusinessId == businessId && t.Status.ToString() == "OPEN").ToListAsync();
        var res = new List<TableSummaryDto>();
        foreach (var t in tables)
        {
            var items = await _ctx.OrderItems.Where(oi => oi.BusinessId == businessId && oi.OrderId != Guid.Empty && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED && _ctx.Orders.Any(o => o.Id == oi.OrderId && o.TableId == t.Id)).ToListAsync();
            // simpler: get items by joining orders
            var orderIds = await _ctx.Orders.Where(o => o.TableId == t.Id).Select(o => o.Id).ToListAsync();
            var relevant = await _ctx.OrderItems.Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED).ToListAsync();
            var total = relevant.Sum(r => r.UnitPriceSnapshot * r.Quantity);
            var count = relevant.Sum(r => r.Quantity);
            // detect pending items (SENT or IN_PROGRESS)
            var hasPending = await _ctx.OrderItems.AnyAsync(oi => orderIds.Contains(oi.OrderId) && (oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.SENT || oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.IN_PROGRESS));
            res.Add(new TableSummaryDto { TableId = t.Id, TableNumber = t.Number, Status = t.Status.ToString(), CurrentTotal = total, ItemsCount = count, HasPendingItems = hasPending });
        }
        return Ok(res);
    }

    [HttpGet("bill")]
    public async Task<IActionResult> GetBill([FromQuery] Guid businessId, [FromQuery] Guid tableId)
    {
        var table = await _ctx.Tables.FindAsync(tableId);
        if (table == null || table.BusinessId != businessId) return NotFound("Table not found");

        var orders = await _ctx.Orders.Where(o => o.TableId == tableId).ToListAsync();
        var orderIds = orders.Select(o => o.Id).ToList();

        var items = await _ctx.OrderItems.Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED).ToListAsync();

        var lines = items.GroupBy(i => new { i.ProductId, i.UnitPriceSnapshot, i.ProductNameSnapshot, i.Area })
            .Select(g => new BillLineDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductNameSnapshot,
                Quantity = g.Sum(x => x.Quantity),
                UnitPrice = g.Key.UnitPriceSnapshot,
                LineTotal = g.Sum(x => x.UnitPriceSnapshot * x.Quantity),
                Area = g.Key.Area.ToString(),
                Status = g.Any(x => x.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.SENT) ? "SENT" : (g.Any(x => x.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.IN_PROGRESS) ? "IN_PROGRESS" : "DONE")
            }).ToList();

        var subtotal = lines.Sum(l => l.LineTotal);
        var tax = 0m; // for now
        var tip = 0m;
        var discount = 0m;
        var total = subtotal + tax + tip - discount;

        var paid = await _ctx.Payments.Where(p => p.TableId == tableId && p.BusinessId == businessId).SumAsync(p => p.Amount);
        var due = total - paid;

        var hasPending = await _ctx.OrderItems.AnyAsync(oi => orderIds.Contains(oi.OrderId) && (oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.SENT || oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.IN_PROGRESS));

        var resp = new GetBillResponse
        {
            BusinessId = businessId,
            TableId = tableId,
            TableNumber = table.Number,
            Lines = lines,
            Subtotal = subtotal,
            Tax = tax,
            Tip = tip,
            Discount = discount,
            Total = total,
            Paid = paid,
            Due = due,
            HasPendingItems = hasPending
        };
        return Ok(resp);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest req)
    {
        if (req.Amount <= 0) return BadRequest("Invalid amount");

        var table = await _ctx.Tables.FindAsync(req.TableId);
        if (table == null || table.BusinessId != req.BusinessId) return BadRequest("Table not found for business");
        if (table.Status.ToString() != "OPEN") return BadRequest("Table is not open");

        // validate shift
        var shift = await _ctx.Shifts.FindAsync(req.ShiftId);
        if (shift == null || shift.BusinessId != req.BusinessId || shift.Status != "OPEN") return BadRequest("Invalid or closed shift");
        if (shift.UserId != req.ActorUserId) return BadRequest("Shift does not belong to actor user");

        if (!Enum.TryParse<BMTECHRD.Pos.Domain.Enums.PaymentMethod>(req.Method, true, out var method)) return BadRequest("Invalid payment method");

        // compute current total and due for validation
        var ordersBefore = await _ctx.Orders.Where(o => o.TableId == req.TableId).ToListAsync();
        var orderIdsBefore = ordersBefore.Select(o => o.Id).ToList();
        var itemsBefore = await _ctx.OrderItems.Where(oi => orderIdsBefore.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED).ToListAsync();
        var subtotalBefore = itemsBefore.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var totalBefore = subtotalBefore; // tax/tip/discount omitted
        var paidBefore = await _ctx.Payments.Where(p => p.TableId == req.TableId && p.BusinessId == req.BusinessId).SumAsync(p => p.Amount);
        var dueBefore = totalBefore - paidBefore;

        // amount validations
        if (req.Amount <= 0) return BadRequest("Amount must be positive");
        if (method != BMTECHRD.Pos.Domain.Enums.PaymentMethod.CASH && req.Amount > dueBefore)
            return BadRequest("Non-cash payments cannot exceed due amount");

        var payment = new BMTECHRD.Pos.Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            TableId = req.TableId,
            ShiftId = req.ShiftId,
            CreatedByUserId = req.ActorUserId,
            Method = method,
            Amount = req.Amount,
            MetaJson = req.Meta != null ? System.Text.Json.JsonSerializer.Serialize(req.Meta) : null,
            CreatedAt = DateTime.UtcNow
        };

        _ctx.Payments.Add(payment);
        await _ctx.SaveChangesAsync();

        // recalc
        var paid = await _ctx.Payments.Where(p => p.TableId == req.TableId && p.BusinessId == req.BusinessId).SumAsync(p => p.Amount);

        // compute current total same as GetBill
        var orders = await _ctx.Orders.Where(o => o.TableId == req.TableId).ToListAsync();
        var orderIds = orders.Select(o => o.Id).ToList();
        var items = await _ctx.OrderItems.Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED).ToListAsync();
        var subtotal = items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var total = subtotal; // tax/tip/discount omitted
        var due = total - paid;

        var closed = false;
        string? closeBlockedReason = null;
        if (req.CloseIfPaid && due <= 0)
        {
            // check pending items
            var hasPending = await _ctx.OrderItems.AnyAsync(oi => orderIds.Contains(oi.OrderId) && (oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.SENT || oi.Status == BMTECHRD.Pos.Domain.Enums.OrderItemStatus.IN_PROGRESS));
            if (hasPending)
            {
                closed = false;
                closeBlockedReason = "Hay pedidos pendientes (Pendiente/En proceso). No se puede cerrar la factura.";
            }
            else
            {
                // close table
                table.Status = BMTECHRD.Pos.Domain.Enums.TableStatus.AVAILABLE;
                table.OpenedAt = null;
                table.OpenedByWaiterId = null;
                table.UpdatedAt = DateTime.UtcNow;
                await _ctx.SaveChangesAsync();
                closed = true;
            }
        }

        // emit events
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated");
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated");

        var change = 0m;
        if (req.CashGiven.HasValue && req.CashGiven.Value > req.Amount)
            change = req.CashGiven.Value - req.Amount;

        var resp = new CreatePaymentResponse { Paid = paid, Due = due, Change = change, Closed = closed, CloseBlockedReason = closeBlockedReason };
        return Ok(resp);
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseTable([FromBody] CloseTableRequest req)
    {
        var table = await _ctx.Tables.FindAsync(req.TableId);
        if (table == null || table.BusinessId != req.BusinessId) return BadRequest("Table not found");

        // simple role check omitted; assume caller allowed for now
        // compute due
        var paid = await _ctx.Payments.Where(p => p.TableId == req.TableId && p.BusinessId == req.BusinessId).SumAsync(p => p.Amount);
        var orders = await _ctx.Orders.Where(o => o.TableId == req.TableId).ToListAsync();
        var orderIds = orders.Select(o => o.Id).ToList();
        var items = await _ctx.OrderItems.Where(oi => orderIds.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED).ToListAsync();
        var subtotal = items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var total = subtotal;
        var due = total - paid;

        if (due > 0) return BadRequest("Due amount must be paid before closing table");

        table.Status = BMTECHRD.Pos.Domain.Enums.TableStatus.AVAILABLE;
        table.OpenedAt = null;
        table.OpenedByWaiterId = null;
        table.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("tables.updated");
        await _hub.Clients.Group(req.BusinessId.ToString()).SendAsync("cash.updated");

        return Ok(new { Closed = true });
    }
}
