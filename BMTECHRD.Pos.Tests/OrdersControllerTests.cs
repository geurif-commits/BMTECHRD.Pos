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

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task CreateBatch_ShouldSplitKitchenAndBarItems_AndDiscountStock()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var ctx = new AppDbContext(options);

        var businessId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var table = new Table
        {
            Id = tableId,
            BusinessId = businessId,
            Number = 1,
            Status = TableStatus.OPEN,
            PosX = 10,
            PosY = 10,
            CreatedAt = DateTime.UtcNow
        };

        var food = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Hamburguesa",
            Price = 250,
            Stock = 10,
            TrackInventory = true,
            Area = ProductionArea.KITCHEN,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var drink = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Limonada",
            Price = 120,
            Stock = 20,
            TrackInventory = true,
            Area = ProductionArea.BAR,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        ctx.Tables.Add(table);
        ctx.Products.AddRange(food, drink);
        await ctx.SaveChangesAsync();

        var hub = new FakeHubContext();
        var controller = new OrdersController(ctx, hub);

        var response = await controller.CreateBatch(new CreateOrderBatchRequest
        {
            BusinessId = businessId,
            TableId = tableId,
            ActorUserId = actorId,
            Items =
            [
                new CreateOrderBatchLine { ProductId = food.Id, Quantity = 2 },
                new CreateOrderBatchLine { ProductId = drink.Id, Quantity = 3 }
            ]
        });

        var ok = Assert.IsType<OkObjectResult>(response);
        var payload = Assert.IsType<CreateOrderBatchResponse>(ok.Value);

        Assert.Equal(5, payload.TotalItems);
        Assert.Equal(2, payload.KitchenItems);
        Assert.Equal(3, payload.BarItems);

        var savedItems = await ctx.OrderItems.ToListAsync();
        Assert.Equal(2, savedItems.Count);
        Assert.Contains(savedItems, x => x.ProductId == food.Id && x.Area == ProductionArea.KITCHEN);
        Assert.Contains(savedItems, x => x.ProductId == drink.Id && x.Area == ProductionArea.BAR);

        var refreshedFood = await ctx.Products.FindAsync(food.Id);
        var refreshedDrink = await ctx.Products.FindAsync(drink.Id);

        Assert.NotNull(refreshedFood);
        Assert.NotNull(refreshedDrink);
        Assert.Equal(8, refreshedFood!.Stock);
        Assert.Equal(17, refreshedDrink!.Stock);
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
