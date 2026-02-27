using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Reports;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class ReportsServiceTests
{
    [Fact]
    public async Task GetSalesByProductAsync_WhenInvalidRange_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var sut = new ReportsService(db);

        var from = DateTime.UtcNow;
        var to = from.AddDays(-1);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.GetSalesByProductAsync(Guid.NewGuid(), Guid.NewGuid(), from, to, 20, CancellationToken.None));

        Assert.Equal("REPORT_INVALID_RANGE", ex.ErrorCode);
        Assert.Equal(400, ex.StatusCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
