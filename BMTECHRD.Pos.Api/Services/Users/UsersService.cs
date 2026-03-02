using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Users;

public sealed class UsersService : IUsersService
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;

    public UsersService(AppDbContext ctx, IPasswordHasher hasher)
    {
        _ctx = ctx;
        _hasher = hasher;
    }

    public async Task<List<UserListItemDto>> GetAsync(Guid businessId, CancellationToken ct)
    {
        var users = await _ctx.Users.AsNoTracking().Where(u => u.BusinessId == businessId).ToListAsync(ct);
        return users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            HasPin = !string.IsNullOrEmpty(u.PinHash),
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<Guid> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        // Check if this is the first user for the business (bootstrap scenario)
        var isFirstUser = !await _ctx.Users.AnyAsync(u => u.BusinessId == req.BusinessId, ct);

        if (!isFirstUser)
        {
            // If not first user, validate actor permissions
            var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
            EnsureAdminOrSupervisor(actor, req.BusinessId);
        }

        if (await _ctx.Users.AnyAsync(u => u.BusinessId == req.BusinessId && u.Username == req.Username, ct))
            throw new ApiProblemException(StatusCodes.Status409Conflict, "Username conflict", "Username already exists for this business", "USER_USERNAME_EXISTS");

        string? pinHash = null;
        if (!string.IsNullOrEmpty(req.Pin4))
        {
            ValidatePin(req.Pin4);
            pinHash = _hasher.Hash(req.Pin4);
        }

        if (!Enum.TryParse<UserRole>(req.Role, true, out var role))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid role", "Invalid role", "USER_ROLE_INVALID");

        var user = new User
        {
            Username = req.Username,
            PasswordHash = _hasher.Hash(req.Password),
            PinHash = pinHash,
            Role = role,
            IsActive = req.IsActive,
            BusinessId = req.BusinessId
        };

        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest req, CancellationToken ct)
    {
        var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
        EnsureAdminOrSupervisor(actor, req.BusinessId);

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId, ct);
        if (user == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "User not found", "User not found", "USER_NOT_FOUND");

        if (!Enum.TryParse<UserRole>(req.Role, true, out var newRole))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid role", "Invalid role", "USER_ROLE_INVALID");

        var actorRole = actor?.Role;
        if (actorRole == UserRole.SUPERVISOR && user.Role == UserRole.ADMIN && newRole != UserRole.ADMIN)
            throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Supervisor cannot demote admin", "USER_DEMOTE_FORBIDDEN");

        if (!req.IsActive && user.Role == UserRole.ADMIN)
        {
            var otherAdmins = await _ctx.Users.CountAsync(u => u.BusinessId == req.BusinessId && u.Role == UserRole.ADMIN && u.IsActive && u.Id != user.Id, ct);
            if (otherAdmins == 0)
                throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid operation", "Cannot deactivate the last active admin", "USER_LAST_ADMIN");
        }

        user.Role = newRole;
        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _ctx.SaveChangesAsync(ct);
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest req, CancellationToken ct)
    {
        var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
        EnsureAdminOrSupervisor(actor, req.BusinessId);

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId, ct);
        if (user == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "User not found", "User not found", "USER_NOT_FOUND");

        user.PasswordHash = _hasher.Hash(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task ResetPinAsync(Guid id, ResetPinRequest req, CancellationToken ct)
    {
        var actor = await _ctx.Users.FindAsync(new object?[] { req.ActorUserId }, ct);
        EnsureAdminOrSupervisor(actor, req.BusinessId);

        ValidatePin(req.NewPin4);

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId, ct);
        if (user == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "User not found", "User not found", "USER_NOT_FOUND");

        user.PinHash = _hasher.Hash(req.NewPin4);
        user.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
    }

    private static void EnsureAdminOrSupervisor(User? actor, Guid businessId)
    {
        if (actor == null || actor.BusinessId != businessId)
            throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor not allowed", "USER_ACTOR_FORBIDDEN");

        if (!(actor.Role == UserRole.ADMIN || actor.Role == UserRole.SUPERVISOR))
            throw new ApiProblemException(StatusCodes.Status403Forbidden, "Forbidden", "Actor role not allowed", "USER_ACTOR_ROLE_FORBIDDEN");
    }

    private static void ValidatePin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid PIN", "Pin must be exactly 4 digits", "USER_PIN_INVALID");
    }
}
