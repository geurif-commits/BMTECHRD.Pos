namespace BMTECHRD.Pos.Api.Services.Idempotency;

public interface IIdempotencyKeyStore
{
    Task<Guid?> TryGetEntityIdAsync(string scope, Guid businessId, Guid actorUserId, string idempotencyKey, CancellationToken ct);
    Task SaveAsync(string scope, Guid businessId, Guid actorUserId, string idempotencyKey, Guid entityId, TimeSpan ttl, CancellationToken ct);
}
