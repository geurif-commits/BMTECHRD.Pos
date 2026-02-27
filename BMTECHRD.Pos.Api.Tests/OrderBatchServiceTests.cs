using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Api.Services.Orders;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class OrderBatchServiceTests
{
    [Fact]
    public async Task CreateBatchAsync_WhenNoItems_ThrowsApiProblemException()
    {
        var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        db.Tables.Add(new BMTECHRD.Pos.Domain.Entities.Table
        {
            Id = tableId,
            BusinessId = businessId,
            Number = 1,
            Status = BMTECHRD.Pos.Domain.Enums.TableStatus.OPEN
        });
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new OrderBatchService(db, hub, new AuditLogIdempotencyKeyStore(db));

        var req = new CreateOrderBatchRequest
        {
            BusinessId = businessId,
            TableId = tableId,
            ActorUserId = Guid.NewGuid(),
            Items = new List<CreateOrderBatchLine>()
        };

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() => sut.CreateBatchAsync(req, null, CancellationToken.None));

        Assert.Equal("ORDER_EMPTY", ex.ErrorCode);
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
