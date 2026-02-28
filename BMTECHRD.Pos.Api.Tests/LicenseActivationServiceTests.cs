using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.License;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class LicenseActivationServiceTests
{
    [Fact]
    public async Task ActivateAsync_WhenKeyMissing_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var sut = new LicenseActivationService(db);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.ActivateAsync(new ActivateLicenseRequest { ActivationKey = "MISSING" }, CancellationToken.None));

        Assert.Equal("LIC_NOT_FOUND", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
}
