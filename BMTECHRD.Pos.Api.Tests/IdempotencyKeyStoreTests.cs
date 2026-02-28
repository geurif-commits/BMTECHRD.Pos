using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BMTECHRD.Pos.Api.Tests;

public class IdempotencyKeyStoreTests
{
    [Fact]
    public async Task SaveAndGet_WithUnexpiredKey_ReturnsEntityId()
    {
        await using var db = CreateDbContext();
        var sut = new AuditLogIdempotencyKeyStore(db);

        var businessId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        await sut.SaveAsync("TEST_SCOPE", businessId, actorUserId, "key-1", entityId, TimeSpan.FromHours(1), CancellationToken.None);

        var found = await sut.TryGetEntityIdAsync("TEST_SCOPE", businessId, actorUserId, "key-1", CancellationToken.None);

        Assert.Equal(entityId, found);
    }

    [Fact]
    public async Task TryGet_WithExpiredKey_ReturnsNull()
    {
        await using var db = CreateDbContext();

        db.AuditLogs.Add(new BMTECHRD.Pos.Domain.Entities.AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            ActorUserId = Guid.NewGuid(),
            Action = "IDEMPOTENCY_KEY",
            EntityType = "TEST_SCOPE",
            EntityId = Guid.NewGuid(),
            DataJson = "{\"key\":\"key-expired\",\"expiresAtUtc\":\"2000-01-01T00:00:00Z\"}",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });

        await db.SaveChangesAsync();

        var row = await db.AuditLogs.AsNoTracking().FirstAsync();

        var sut = new AuditLogIdempotencyKeyStore(db);
        var found = await sut.TryGetEntityIdAsync("TEST_SCOPE", row.BusinessId, row.ActorUserId ?? Guid.Empty, "key-expired", CancellationToken.None);

        Assert.Null(found);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
