using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.InventoryMovements;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class InventoryMovementsServiceTests
{
    [Fact]
    public async Task AdjustAsync_WhenActorRoleForbidden_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        db.Users.Add(new BMTECHRD.Pos.Domain.Entities.User
        {
            Id = actorId,
            BusinessId = businessId,
            Username = "waiter",
            PasswordHash = "hash",
            Role = BMTECHRD.Pos.Domain.Enums.UserRole.WAITER
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
            Area = BMTECHRD.Pos.Domain.Enums.ProductionArea.BAR,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new InventoryMovementsService(db, hub);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() => sut.AdjustAsync(new InventoryAdjustRequest
        {
            BusinessId = businessId,
            ProductId = productId,
            QuantityDelta = -1,
            Reason = "test",
            ActorUserId = actorId
        }, CancellationToken.None));

        Assert.Equal("INV_ACTOR_ROLE_FORBIDDEN", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
}
