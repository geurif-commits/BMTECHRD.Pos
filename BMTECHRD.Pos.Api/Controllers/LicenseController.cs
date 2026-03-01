using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
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
        if (request is null || string.IsNullOrWhiteSpace(request.ActivationKey))
            return BadRequest("Activation key is required");

        var normalizedKey = request.ActivationKey.Trim();

        var license = await _ctx.Licenses
            .FirstOrDefaultAsync(x => x.ActivationKey.ToUpper() == normalizedKey.ToUpper());
        if (license == null) return NotFound("License key not found");

        if (!license.IsActive(DateTime.UtcNow))
        {
            license.Status = LicenseStatus.ACTIVE;
            license.ActivatedAt = DateTime.UtcNow;
            switch (license.Plan)
            {
                case LicensePlan.TRIAL_7_DAYS:
                    license.ExpiresAt = DateTime.UtcNow.AddDays(7);
                    break;
                case LicensePlan.MONTHS_6:
                    license.ExpiresAt = DateTime.UtcNow.AddMonths(6);
                    break;
                case LicensePlan.MONTHS_12:
                    license.ExpiresAt = DateTime.UtcNow.AddMonths(12);
                    break;
                case LicensePlan.LIFETIME:
                    license.ExpiresAt = null;
                    break;
                default:
                    license.ExpiresAt = DateTime.UtcNow.AddDays(7);
                    break;
            }

            _ctx.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                BusinessId = license.BusinessId,
                ActorUserId = null,
                Action = "LICENSE_ACTIVATED",
                EntityType = "License",
                EntityId = license.Id,
                DataJson = $"{{\"plan\":\"{license.Plan}\",\"expiresAt\":\"{license.ExpiresAt:o}\"}}",
                CreatedAt = DateTime.UtcNow
            });

            await _ctx.SaveChangesAsync();
        }

        return Ok(ToActivateResponse(license));
    }

    [HttpGet("status")]
    [Authorize(Policy = "SupervisorOrAdmin")]
    public async Task<IActionResult> GetStatus([FromQuery] Guid businessId)
    {
        if (businessId == Guid.Empty) return BadRequest("businessId is required");

        var license = await EnsureLicenseForBusinessAsync(businessId);
        if (license == null) return NotFound("Business not found");
        return Ok(ToStatusResponse(license));
    }

    [HttpGet("alerts")]
    [Authorize(Policy = "SupervisorOrAdmin")]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid businessId, [FromQuery] int warningDays = 10)
    {
        if (businessId == Guid.Empty) return BadRequest("businessId is required");
        if (warningDays < 1) warningDays = 1;

        var license = await EnsureLicenseForBusinessAsync(businessId);
        if (license == null) return NotFound("Business not found");

        var status = ToStatusResponse(license);
        var alert = new
        {
            businessId,
            status.Status,
            status.Plan,
            status.IsActive,
            status.ExpiresAt,
            status.DaysRemaining,
            warningDays,
            shouldAlert = !status.IsActive || (status.DaysRemaining.HasValue && status.DaysRemaining.Value <= warningDays),
            message = !status.IsActive
                ? "La licencia no está activa."
                : (status.DaysRemaining.HasValue && status.DaysRemaining.Value <= warningDays)
                    ? $"La licencia expira en {status.DaysRemaining} día(s)."
                    : "Licencia en estado saludable."
        };

        return Ok(alert);
    }

    private async Task<License?> EnsureLicenseForBusinessAsync(Guid businessId)
    {
        var license = await _ctx.Licenses.FirstOrDefaultAsync(x => x.BusinessId == businessId);
        if (license != null) return license;

        var business = await _ctx.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);
        if (business == null) return null;

        license = new License
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Business = business,
            Plan = LicensePlan.TRIAL_7_DAYS,
            Status = LicenseStatus.INACTIVE,
            ActivationKey = $"BMT-{businessId:N}"[..14].ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _ctx.Licenses.Add(license);
        await _ctx.SaveChangesAsync();
        return license;
    }

    private static ActivateLicenseResponse ToActivateResponse(License license)
    {
        var now = DateTime.UtcNow;
        var isActive = license.IsActive(now);
        var days = license.ExpiresAt.HasValue ? (int?)Math.Max(0, (license.ExpiresAt.Value - now).Days) : null;

        return new ActivateLicenseResponse
        {
            Status = license.Status.ToString(),
            Plan = license.Plan.ToString(),
            ActivatedAt = license.ActivatedAt,
            ExpiresAt = license.ExpiresAt,
            IsActive = isActive,
            DaysRemaining = days
        };
    }

    private static LicenseStatusResponse ToStatusResponse(License license)
    {
        var now = DateTime.UtcNow;
        var isActive = license.IsActive(now);
        var days = license.ExpiresAt.HasValue ? (int?)Math.Max(0, (license.ExpiresAt.Value - now).Days) : null;

        return new LicenseStatusResponse
        {
            BusinessId = license.BusinessId,
            Status = license.Status.ToString(),
            Plan = license.Plan.ToString(),
            ActivatedAt = license.ActivatedAt,
            ExpiresAt = license.ExpiresAt,
            IsActive = isActive,
            DaysRemaining = days,
            IsExpiringSoon = isActive && days.HasValue && days.Value <= 10
        };
    }
}
