using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
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
            CurrencyCode = "DOP"
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
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(logo.FileName)}";
            var relative = $"/uploads/logos/{fileName}";
            var physical = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos", fileName);
            using var stream = System.IO.File.Create(physical);
            await logo.CopyToAsync(stream);
            business.LogoPath = relative;
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

    private static string GenerateActivationKey()
    {
        static string Part()
        {
            var rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0,5).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }
        return $"BMT-{Part()}-{Part()}";
    }
}
