using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Api.Services.Shifts;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class ShiftServiceTests
{
    [Fact]
    public async Task ListAsync_WhenActorRoleIsNotAllowed_ThrowsApiProblemException()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        db.Users.Add(new BMTECHRD.Pos.Domain.Entities.User
        {
            Id = actorId,
            BusinessId = businessId,
            Username = "cashier1",
            PasswordHash = "hash",
            Role = BMTECHRD.Pos.Domain.Enums.UserRole.CASHIER
        });
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new ShiftService(db, hub, new AuditLogIdempotencyKeyStore(db));

        var ex = await Assert.ThrowsAsync<ApiProblemException>(() =>
            sut.ListAsync(businessId, null, null, null, null, 100, actorId, CancellationToken.None));

        Assert.Equal("SHIFT_ACTOR_ROLE_FORBIDDEN", ex.ErrorCode);
        Assert.Equal(403, ex.StatusCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
