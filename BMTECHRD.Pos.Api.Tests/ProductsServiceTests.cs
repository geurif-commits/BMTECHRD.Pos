using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Products;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class ProductsServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenCategoryMissing_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var sut = new ProductsService(db);

        var product = new BMTECHRD.Pos.Domain.Entities.Product
        {
            BusinessId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Cafe",
            Price = 100m,
            Stock = 10,
            TrackInventory = true,
            Area = BMTECHRD.Pos.Domain.Enums.ProductionArea.BAR,
            IsActive = true
        };

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() => sut.CreateAsync(product, CancellationToken.None));
        Assert.Equal("PROD_CATEGORY_NOT_FOUND", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
