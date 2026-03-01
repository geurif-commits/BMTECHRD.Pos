using BMTECHRD.Pos.Api.Controllers;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Tests;

public sealed class CashierControllerTests
{
    [Fact]
    public async Task GetBill_ShouldApplyTipAndItbis_WhenEnabled()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);
        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        ctx.Businesses.Add(new Business
        {
            Id = businessId,
            Name = "Demo",
            CurrencyCode = "DOP",
            EnableItbis = true,
            ItbisRate = 0.18m,
            EnableTip = true,
            TipRate = 0.10m,
            CreatedAt = DateTime.UtcNow
        });

        ctx.Tables.Add(new Table
        {
            Id = tableId,
            BusinessId = businessId,
            Number = 1,
            Status = TableStatus.OPEN,
            PosX = 0,
            PosY = 0,
            CreatedAt = DateTime.UtcNow
        });

        ctx.Orders.Add(new Order
        {
            Id = orderId,
            BusinessId = businessId,
            TableId = tableId,
            CreatedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });

        ctx.OrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderId = orderId,
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Mofongo",
            UnitPriceSnapshot = 100m,
            Quantity = 2,
            Area = ProductionArea.KITCHEN,
            Status = OrderItemStatus.DONE,
            CreatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var controller = new CashierController(ctx, new FakeHubContext());
        var result = await controller.GetBill(businessId, tableId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var bill = Assert.IsType<GetBillResponse>(ok.Value);

        Assert.Equal(200m, bill.Subtotal);
        Assert.Equal(36m, bill.Tax);
        Assert.Equal(20m, bill.Tip);
        Assert.Equal(256m, bill.Total);
    }

    private sealed class FakeHubContext : IHubContext<PosHub>
    {
        public IHubClients Clients { get; } = new FakeHubClients();
        public IGroupManager Groups { get; } = new FakeGroupManager();
    }

    private sealed class FakeHubClients : IHubClients
    {
        private readonly IClientProxy _proxy = new FakeClientProxy();
        public IClientProxy All => _proxy;
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => _proxy;
        public IClientProxy Client(string connectionId) => _proxy;
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => _proxy;
        public IClientProxy Group(string groupName) => _proxy;
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => _proxy;
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => _proxy;
        public IClientProxy User(string userId) => _proxy;
        public IClientProxy Users(IReadOnlyList<string> userIds) => _proxy;
    }

    private sealed class FakeGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
