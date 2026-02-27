using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Production;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class ProductionQueueServiceTests
{
    [Fact]
    public async Task UpdateStatusAsync_InvalidTransition_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        db.OrderItems.Add(new OrderItem
        {
            Id = itemId,
            BusinessId = businessId,
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Pizza",
            UnitPriceSnapshot = 100,
            Quantity = 1,
            Area = ProductionArea.KITCHEN,
            Status = OrderItemStatus.SENT,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = new ProductionQueueService(db, new Mock<IHubContext<PosHub>>().Object);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.UpdateStatusAsync(itemId, "DONE", ProductionArea.KITCHEN, CancellationToken.None));

        Assert.Equal("PROD_STATUS_TRANSITION_INVALID", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
