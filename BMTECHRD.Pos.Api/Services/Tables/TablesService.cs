using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Tables;

public sealed class TablesService : ITablesService
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;
    private readonly ITableAccessPolicy _policy;

    public TablesService(AppDbContext ctx, IPasswordHasher hasher, ITableAccessPolicy policy)
    {
        _ctx = ctx;
        _hasher = hasher;
        _policy = policy;
    }

    public Task<List<Table>> GetAsync(Guid businessId, CancellationToken ct)
        => _ctx.Tables.AsNoTracking().Where(t => t.BusinessId == businessId).OrderBy(t => t.Number).ToListAsync(ct);

    public async Task<Table> OpenAsync(Guid id, OpenTableRequest req, CancellationToken ct)
    {
        var table = await _ctx.Tables.FindAsync(new object?[] { id }, ct);
        if (table == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Table not found", "Table not found", "TABLE_NOT_FOUND");

        if (table.Status != TableStatus.AVAILABLE)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Table unavailable", "Table not available", "TABLE_NOT_AVAILABLE");

        table.Status = TableStatus.OPEN;
        table.OpenedByWaiterId = req.WaiterId;
        table.OpenedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        return table;
    }

    public async Task<TableAccessResponse> AccessAsync(Guid id, TableAccessRequest req, CancellationToken ct)
    {
        var table = await _ctx.Tables.FindAsync(new object?[] { id }, ct);
        if (table == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Table not found", "Table not found", "TABLE_NOT_FOUND");

        var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
        if (actor == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Actor not found", "Actor user not found", "TABLE_ACTOR_NOT_FOUND");

        var requiresPin = _policy.RequiresPin(actor.Role, table.OpenedByWaiterId, req.ActorUserId);
        if (!requiresPin)
            return new TableAccessResponse { RequiresPin = false, AccessGranted = true };

        if (string.IsNullOrWhiteSpace(req.Pin) || req.Pin.Length != 4 || !req.Pin.All(char.IsDigit))
            return new TableAccessResponse { RequiresPin = true, AccessGranted = false, Reason = "Invalid PIN format" };

        var ok = _hasher.Verify(req.Pin, actor.PinHash ?? string.Empty);
        if (!ok)
            return new TableAccessResponse { RequiresPin = true, AccessGranted = false, Reason = "Invalid PIN" };

        return new TableAccessResponse { RequiresPin = true, AccessGranted = true };
    }

    public async Task<Table> UpdatePositionAsync(Guid id, UpdateTablePositionRequest req, CancellationToken ct)
    {
        var table = await _ctx.Tables.FindAsync(new object?[] { id }, ct);
        if (table == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Table not found", "Table not found", "TABLE_NOT_FOUND");

        table.PosX = req.PosX;
        table.PosY = req.PosY;
        await _ctx.SaveChangesAsync(ct);

        return table;
    }
}
