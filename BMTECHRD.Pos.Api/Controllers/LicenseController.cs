using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/license")]
public sealed class LicenseController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public LicenseController(AppDbContext ctx) => _ctx = ctx;

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
    {
        var license = await _ctx.Licenses.FirstOrDefaultAsync(x => x.ActivationKey == request.ActivationKey);
        if (license == null) return NotFound();

        if (license.IsActive(DateTime.UtcNow))
        {
            return Ok(new ActivateLicenseResponse { Status = license.Status.ToString(), Plan = license.Plan.ToString(), ExpiresAt = license.ExpiresAt });
        }

        license.Status = BMTECHRD.Pos.Domain.Enums.LicenseStatus.ACTIVE;
        license.ActivatedAt = DateTime.UtcNow;
        switch (license.Plan)
        {
            case BMTECHRD.Pos.Domain.Enums.LicensePlan.TRIAL_7_DAYS:
                license.ExpiresAt = DateTime.UtcNow.AddDays(7);
                break;
            case BMTECHRD.Pos.Domain.Enums.LicensePlan.MONTHS_6:
                license.ExpiresAt = DateTime.UtcNow.AddMonths(6);
                break;
            case BMTECHRD.Pos.Domain.Enums.LicensePlan.MONTHS_12:
                license.ExpiresAt = DateTime.UtcNow.AddMonths(12);
                break;
            case BMTECHRD.Pos.Domain.Enums.LicensePlan.LIFETIME:
                license.ExpiresAt = null;
                break;
            default:
                license.ExpiresAt = DateTime.UtcNow.AddDays(7);
                break;
        }

        await _ctx.SaveChangesAsync();

        return Ok(new ActivateLicenseResponse { Status = license.Status.ToString(), Plan = license.Plan.ToString(), ExpiresAt = license.ExpiresAt });
    }
}
