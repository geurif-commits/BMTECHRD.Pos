using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Api.Services.Shifts;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class ShiftIdempotencyTests
{
    [Fact]
    public async Task OpenAsync_WithSameIdempotencyKey_DoesNotDuplicateShift()
    {
        await using var db = CreateDbContext();
        var businessId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Users.Add(new BMTECHRD.Pos.Domain.Entities.User
        {
            Id = userId,
            BusinessId = businessId,
            Username = "cashier",
            PasswordHash = "hash",
            Role = BMTECHRD.Pos.Domain.Enums.UserRole.CASHIER
        });
        await db.SaveChangesAsync();

        var hub = new Mock<IHubContext<PosHub>>().Object;
        var sut = new ShiftService(db, hub, new AuditLogIdempotencyKeyStore(db));

        var req = new CreateShiftRequest
        {
            BusinessId = businessId,
            UserId = userId,
            OpeningCash = 100
        };

        await sut.OpenAsync(req, "idem-shift-open-1", CancellationToken.None);
        await sut.OpenAsync(req, "idem-shift-open-1", CancellationToken.None);

        Assert.Equal(1, await db.Shifts.CountAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
