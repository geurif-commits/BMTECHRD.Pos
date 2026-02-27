using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Cashier;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class CashierServiceTests
{
    [Fact]
    public async Task CreatePaymentAsync_WithSameIdempotencyKey_DoesNotDuplicatePayment()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SeedBasicData(db, businessId, tableId, shiftId, userId);
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new CashierService(db, hub);

        var req = new CreatePaymentRequest
        {
            BusinessId = businessId,
            TableId = tableId,
            ShiftId = shiftId,
            ActorUserId = userId,
            Method = "CASH",
            Amount = 20m,
            CloseIfPaid = false
        };

        await sut.CreatePaymentAsync(req, "idem-1", CancellationToken.None);
        await sut.CreatePaymentAsync(req, "idem-1", CancellationToken.None);

        var paymentCount = await db.Payments.CountAsync();
        Assert.Equal(1, paymentCount);
    }

    [Fact]
    public async Task CloseTableAsync_WhenDuePending_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SeedBasicData(db, businessId, tableId, shiftId, userId);
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new CashierService(db, hub);

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.CloseTableAsync(new CloseTableRequest { BusinessId = businessId, TableId = tableId, ActorUserId = userId }, CancellationToken.None));

        Assert.Equal("CASH_DUE_PENDING", ex.ErrorCode);
    }

    private static void SeedBasicData(AppDbContext db, Guid businessId, Guid tableId, Guid shiftId, Guid userId)
    {
        db.Tables.Add(new Table
        {
            Id = tableId,
            BusinessId = businessId,
            Number = 1,
            Status = TableStatus.OPEN
        });

        db.Users.Add(new User
        {
            Id = userId,
            BusinessId = businessId,
            Username = "cashier",
            PasswordHash = "hash",
            Role = UserRole.CASHIER
        });

        db.Shifts.Add(new Shift
        {
            Id = shiftId,
            BusinessId = businessId,
            UserId = userId,
            Status = "OPEN",
            OpenedAt = DateTime.UtcNow,
            OpeningCash = 0m
        });

        var orderId = Guid.NewGuid();
        db.Orders.Add(new Order { Id = orderId, BusinessId = businessId, TableId = tableId, CreatedByUserId = userId, CreatedAt = DateTime.UtcNow });
        db.OrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderId = orderId,
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "item",
            UnitPriceSnapshot = 50m,
            Quantity = 1,
            Area = ProductionArea.BAR,
            Status = OrderItemStatus.DONE,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
