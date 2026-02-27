using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Categories;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class CategoriesServiceTests
{
    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        db.Categories.Add(new BMTECHRD.Pos.Domain.Entities.Category { Id = Guid.NewGuid(), BusinessId = businessId, Name = "Bebidas" });
        await db.SaveChangesAsync();

        var sut = new CategoriesService(db);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.CreateAsync(new BMTECHRD.Pos.Domain.Entities.Category { BusinessId = businessId, Name = "Bebidas" }, CancellationToken.None));

        Assert.Equal("CAT_DUPLICATE_NAME", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
}
