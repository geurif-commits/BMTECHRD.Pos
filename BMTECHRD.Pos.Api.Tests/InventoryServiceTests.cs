using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Inventory;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task AdjustAsync_WhenStockWouldBeNegative_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        db.Products.Add(new BMTECHRD.Pos.Domain.Entities.Product
        {
            Id = productId,
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Agua",
            Price = 100,
            Stock = 1,
            TrackInventory = true,
            Area = BMTECHRD.Pos.Domain.Enums.ProductionArea.BAR
        });
        await db.SaveChangesAsync();

        var sut = new InventoryService(db);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.AdjustAsync(businessId, productId, -2, "TEST", null, CancellationToken.None));

        Assert.Equal("INV_STOCK_INSUFFICIENT", ex.ErrorCode);
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
