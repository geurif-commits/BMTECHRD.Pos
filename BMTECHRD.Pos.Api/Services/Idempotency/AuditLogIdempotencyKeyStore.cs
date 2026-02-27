using System.Text.Json;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Idempotency;

public sealed class AuditLogIdempotencyKeyStore : IIdempotencyKeyStore
{
    private const string ActionName = "IDEMPOTENCY_KEY";
    private readonly AppDbContext _ctx;

    public AuditLogIdempotencyKeyStore(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Guid?> TryGetEntityIdAsync(string scope, Guid businessId, Guid actorUserId, string idempotencyKey, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var records = await _ctx.AuditLogs.AsNoTracking()
            .Where(a => a.Action == ActionName
                        && a.EntityType == scope
                        && a.BusinessId == businessId
                        && a.ActorUserId == actorUserId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var record in records)
        {
            if (record.DataJson is null)
                continue;

            IdempotencyPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<IdempotencyPayload>(record.DataJson);
            }
            catch
            {
                continue;
            }

            if (payload is null || payload.ExpiresAtUtc <= now)
                continue;

            if (string.Equals(payload.Key, idempotencyKey, StringComparison.Ordinal))
                return record.EntityId;
        }

        return null;
    }

    public async Task SaveAsync(string scope, Guid businessId, Guid actorUserId, string idempotencyKey, Guid entityId, TimeSpan ttl, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Compacta llaves expiradas del mismo scope para evitar crecimiento sin control.
        var expirationCutoff = now.AddDays(-7);
        var staleRecords = await _ctx.AuditLogs
            .Where(a => a.Action == ActionName
                        && a.EntityType == scope
                        && a.BusinessId == businessId
                        && a.ActorUserId == actorUserId
                        && a.CreatedAt < expirationCutoff)
            .Take(200)
            .ToListAsync(ct);

        if (staleRecords.Count > 0)
            _ctx.AuditLogs.RemoveRange(staleRecords);

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ActorUserId = actorUserId,
            Action = ActionName,
            EntityType = scope,
            EntityId = entityId,
            DataJson = JsonSerializer.Serialize(new IdempotencyPayload
            {
                Key = idempotencyKey,
                ExpiresAtUtc = now.Add(ttl)
            }),
            CreatedAt = now
        });

        await _ctx.SaveChangesAsync(ct);
    }

    private sealed class IdempotencyPayload
    {
        public string Key { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
