using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/fiscal")]
[Authorize(Policy = "CashierOrAbove")]
public sealed class FiscalController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public FiscalController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueFiscalDocumentRequest req)
    {
        var business = await _ctx.Businesses.FirstOrDefaultAsync(x => x.Id == req.BusinessId);
        if (business == null) return NotFound("Business not found");

        var table = await _ctx.Tables.FirstOrDefaultAsync(x => x.Id == req.TableId && x.BusinessId == req.BusinessId);
        if (table == null) return NotFound("Table not found");

        var orders = await _ctx.Orders.Where(o => o.TableId == req.TableId).Select(o => o.Id).ToListAsync();
        if (!orders.Any()) return BadRequest("No orders for table");

        var items = await _ctx.OrderItems
            .Where(oi => orders.Contains(oi.OrderId) && oi.Status != BMTECHRD.Pos.Domain.Enums.OrderItemStatus.CANCELLED)
            .ToListAsync();

        var subtotal = items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var tax = business.EnableItbis ? Math.Round(subtotal * business.ItbisRate, 2) : 0m;
        var tip = business.EnableTip ? Math.Round(subtotal * business.TipRate, 2) : 0m;
        var total = subtotal + tax + tip;

        var paid = await _ctx.Payments.Where(p => p.BusinessId == req.BusinessId && p.TableId == req.TableId).SumAsync(p => p.Amount);
        if (paid < total) return BadRequest("Table is not fully paid");

        var type = string.Equals(req.Type, "ECF", StringComparison.OrdinalIgnoreCase) ? "ECF" : "NFC";
        if (type == "ECF" && !business.EnableElectronicInvoice) return BadRequest("Electronic invoicing is disabled");
        if (type == "NFC" && !business.EnableFiscalReceipt) return BadRequest("Fiscal receipt is disabled");

        var prefix = type == "ECF" ? "E31" : "B02";
        var issuedCount = await _ctx.FiscalDocuments.CountAsync(x => x.BusinessId == req.BusinessId && x.Type == type);
        var number = $"{prefix}{(issuedCount + 1).ToString().PadLeft(8, '0')}";

        var doc = new FiscalDocument
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            TableId = req.TableId,
            Type = type,
            Number = number,
            Subtotal = subtotal,
            Tax = tax,
            Tip = tip,
            Total = total,
            IssuedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Status = "ISSUED"
        };

        _ctx.FiscalDocuments.Add(doc);
        await _ctx.SaveChangesAsync();

        return Ok(new FiscalDocumentResponse
        {
            Id = doc.Id,
            Type = doc.Type,
            Number = doc.Number,
            Subtotal = doc.Subtotal,
            Tax = doc.Tax,
            Tip = doc.Tip,
            Total = doc.Total,
            IssuedAt = doc.IssuedAt
        });
    }
}
