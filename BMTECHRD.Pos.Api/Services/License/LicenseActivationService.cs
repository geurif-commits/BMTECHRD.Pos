using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.License;

public sealed class LicenseActivationService : ILicenseActivationService
{
    private readonly AppDbContext _ctx;

    public LicenseActivationService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<ActivateLicenseResponse> ActivateAsync(ActivateLicenseRequest request, CancellationToken ct)
    {
        var license = await _ctx.Licenses.FirstOrDefaultAsync(x => x.ActivationKey == request.ActivationKey, ct);
        if (license == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "License not found", "License not found", "LIC_NOT_FOUND");

        if (!license.IsActive(DateTime.UtcNow))
        {
            license.Status = LicenseStatus.ACTIVE;
            license.ActivatedAt = DateTime.UtcNow;
            license.ExpiresAt = license.Plan switch
            {
                LicensePlan.TRIAL_7_DAYS => DateTime.UtcNow.AddDays(7),
                LicensePlan.MONTHS_6 => DateTime.UtcNow.AddMonths(6),
                LicensePlan.MONTHS_12 => DateTime.UtcNow.AddMonths(12),
                LicensePlan.LIFETIME => null,
                _ => DateTime.UtcNow.AddDays(7)
            };

            await _ctx.SaveChangesAsync(ct);
        }

        return new ActivateLicenseResponse
        {
            Status = license.Status.ToString(),
            Plan = license.Plan.ToString(),
            ExpiresAt = license.ExpiresAt
        };
    }
}
