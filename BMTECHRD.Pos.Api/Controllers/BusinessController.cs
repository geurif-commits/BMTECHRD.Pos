using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Authorize(Policy = "SupervisorOrAdmin")]
[Route("api/business")]
public sealed class BusinessController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public BusinessController(AppDbContext ctx) => _ctx = ctx;

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateBusinessRequest request)
    {
        var form = await Request.ReadFormAsync();
        var name = request.Name;
        IFormFile? logo = form.Files.GetFile("logo");

        var business = new Business
        {
            Name = name,
            CurrencyCode = "DOP",
            EnableItbis = true,
            ItbisRate = 0.18m,
            EnableTip = true,
            TipRate = 0.10m,
            EnableFiscalReceipt = false,
            EnableElectronicInvoice = false
        };

        var activationKey = GenerateActivationKey();
        var license = new License
        {
            ActivationKey = activationKey,
            Plan = BMTECHRD.Pos.Domain.Enums.LicensePlan.TRIAL_7_DAYS,
            Status = BMTECHRD.Pos.Domain.Enums.LicenseStatus.INACTIVE,
            Business = business
        };

        business.License = license;

        if (logo != null && logo.Length > 0)
        {
            business.LogoPath = await SaveLogoAsync(logo);
        }

        _ctx.Businesses.Add(business);
        await _ctx.SaveChangesAsync();

        var response = new CreateBusinessResponse
        {
            BusinessId = business.Id,
            Name = business.Name,
            LogoPath = business.LogoPath,
            ActivationKey = activationKey
        };

        return CreatedAtAction(null, response);
    }

    [HttpGet("{businessId:guid}/settings")]
    public async Task<IActionResult> GetSettings([FromRoute] Guid businessId)
    {
        var business = await _ctx.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
        if (business == null) return NotFound();

        return Ok(new BusinessSettingsDto
        {
            BusinessId = business.Id,
            Name = business.Name,
            LogoPath = business.LogoPath,
            EnableItbis = business.EnableItbis,
            ItbisRate = business.ItbisRate,
            EnableTip = business.EnableTip,
            TipRate = business.TipRate,
            EnableFiscalReceipt = business.EnableFiscalReceipt,
            EnableElectronicInvoice = business.EnableElectronicInvoice
        });
    }

    [HttpPut("{businessId:guid}/settings")]
    public async Task<IActionResult> UpdateSettings([FromRoute] Guid businessId, [FromForm] UpdateBusinessSettingsRequest request)
    {
        var business = await _ctx.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
        if (business == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Business name is required");
        if (request.EnableItbis && (request.ItbisRate < 0 || request.ItbisRate > 1)) return BadRequest("ITBIS rate must be between 0 and 1");
        if (request.EnableTip && (request.TipRate < 0 || request.TipRate > 1)) return BadRequest("Tip rate must be between 0 and 1");

        var form = await Request.ReadFormAsync();
        var logo = form.Files.GetFile("logo");

        business.Name = request.Name.Trim();
        business.EnableItbis = request.EnableItbis;
        business.ItbisRate = request.ItbisRate;
        business.EnableTip = request.EnableTip;
        business.TipRate = request.TipRate;
        business.EnableFiscalReceipt = request.EnableFiscalReceipt;
        business.EnableElectronicInvoice = request.EnableElectronicInvoice;

        if (logo != null && logo.Length > 0)
        {
            business.LogoPath = await SaveLogoAsync(logo);
        }

        business.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        return Ok();
    }

    private static async Task<string> SaveLogoAsync(IFormFile logo)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(logo.FileName)}";
        var relative = $"/uploads/logos/{fileName}";
        var physical = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(physical)!);
        await using var stream = System.IO.File.Create(physical);
        await logo.CopyToAsync(stream);
        return relative;
    }

    private static string GenerateActivationKey()
    {
        static string Part()
        {
            var rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 5).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }
        return $"BMT-{Part()}-{Part()}";
    }
}
