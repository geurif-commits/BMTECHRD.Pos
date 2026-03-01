using BMTECHRD.Pos.Api.Controllers;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Tests;

public sealed class LicenseControllerTests
{
    [Fact]
    public async Task Activate_ShouldSetActiveStatusAndExpiration_ForTrial()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Demo",
            CurrencyCode = "DOP",
            CreatedAt = DateTime.UtcNow
        };

        var license = new License
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Business = business,
            ActivationKey = "TEST-KEY",
            Plan = LicensePlan.TRIAL_7_DAYS,
            Status = LicenseStatus.INACTIVE,
            CreatedAt = DateTime.UtcNow
        };

        ctx.Businesses.Add(business);
        ctx.Licenses.Add(license);
        await ctx.SaveChangesAsync();

        var controller = new LicenseController(ctx);
        var result = await controller.Activate(new ActivateLicenseRequest { ActivationKey = "TEST-KEY" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ActivateLicenseResponse>(ok.Value);

        Assert.True(payload.IsActive);
        Assert.Equal("ACTIVE", payload.Status);
        Assert.NotNull(payload.ExpiresAt);
        Assert.True(payload.DaysRemaining >= 6);

        var logs = await ctx.AuditLogs.Where(x => x.Action == "LICENSE_ACTIVATED").ToListAsync();
        Assert.Single(logs);
    }
}
