using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Services.Users;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class UsersServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenActorNotAllowed_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var hasher = new PasswordHasher();
        var businessId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        db.Users.Add(new BMTECHRD.Pos.Domain.Entities.User
        {
            Id = actorId,
            BusinessId = businessId,
            Username = "waiter",
            PasswordHash = "hash",
            Role = BMTECHRD.Pos.Domain.Enums.UserRole.WAITER,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var sut = new UsersService(db, hasher);

        var req = new BMTECHRD.Pos.Application.DTOs.CreateUserRequest
        {
            BusinessId = businessId,
            ActorUserId = actorId,
            Username = "new-user",
            Password = "123456",
            Role = "CASHIER",
            IsActive = true
        };

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() => sut.CreateAsync(req, CancellationToken.None));
        Assert.Equal("USER_ACTOR_ROLE_FORBIDDEN", ex.ErrorCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
