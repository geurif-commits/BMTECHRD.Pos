using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Business;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class BusinessServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenNameMissing_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var sut = new BusinessService(db);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.CreateAsync(new CreateBusinessRequest { Name = "" }, null, CancellationToken.None));

        Assert.Equal("BIZ_NAME_REQUIRED", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
}
