using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Tables;
using BMTECHRD.Pos.Infrastructure.Auth;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class TablesServiceTests
{
    [Fact]
    public async Task OpenAsync_WhenTableNotAvailable_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var hasher = new PasswordHasher();
        var policy = new TableAccessPolicy();
        var tableId = Guid.NewGuid();

        db.Tables.Add(new BMTECHRD.Pos.Domain.Entities.Table
        {
            Id = tableId,
            BusinessId = Guid.NewGuid(),
            Number = 1,
            Status = BMTECHRD.Pos.Domain.Enums.TableStatus.OPEN
        });
        await db.SaveChangesAsync();

        var sut = new TablesService(db, hasher, policy);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.OpenAsync(tableId, new BMTECHRD.Pos.Application.DTOs.OpenTableRequest { WaiterId = Guid.NewGuid() }, CancellationToken.None));

        Assert.Equal("TABLE_NOT_AVAILABLE", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
