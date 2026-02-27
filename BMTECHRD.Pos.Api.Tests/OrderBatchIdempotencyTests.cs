using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Orders;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class OrderBatchIdempotencyTests
{
    [Fact]
    public async Task CreateBatchAsync_WithSameIdempotencyKey_DoesNotDuplicateOrder()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();

        db.Tables.Add(new BMTECHRD.Pos.Domain.Entities.Table
        {
            Id = tableId,
            BusinessId = businessId,
            Number = 1,
            Status = TableStatus.OPEN
        });

        db.Products.Add(new BMTECHRD.Pos.Domain.Entities.Product
        {
            Id = productId,
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Cafe",
            Price = 100,
            Stock = 10,
            TrackInventory = true,
            Area = ProductionArea.BAR,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new OrderBatchService(db, hub);

        var req = new CreateOrderBatchRequest
        {
            BusinessId = businessId,
            TableId = tableId,
            ActorUserId = actorUserId,
            Items = new List<CreateOrderBatchLine>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        await sut.CreateBatchAsync(req, "idem-order-1", CancellationToken.None);
        await sut.CreateBatchAsync(req, "idem-order-1", CancellationToken.None);

        Assert.Equal(1, await db.Orders.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
