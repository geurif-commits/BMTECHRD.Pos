using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;

    public UsersController(AppDbContext ctx, IPasswordHasher hasher)
    {
        _ctx = ctx;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId)
    {
        var users = await _ctx.Users.Where(u => u.BusinessId == businessId).ToListAsync();
        var list = users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            HasPin = !string.IsNullOrEmpty(u.PinHash),
            CreatedAt = u.CreatedAt
        }).ToList();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        // validate actor
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null || actor.BusinessId != req.BusinessId) return Forbid();
        if (!(actor.Role == UserRole.ADMIN || actor.Role == UserRole.SUPERVISOR)) return Forbid();

        // username unique per business
        if (await _ctx.Users.AnyAsync(u => u.BusinessId == req.BusinessId && u.Username == req.Username))
            return Conflict("Username already exists for this business");

        // validate pin if provided
        string? pinHash = null;
        if (!string.IsNullOrEmpty(req.Pin4))
        {
            if (req.Pin4.Length != 4 || !req.Pin4.All(char.IsDigit)) return BadRequest("Pin must be exactly 4 digits");
            pinHash = _hasher.Hash(req.Pin4);
        }

        // map role
        if (!Enum.TryParse<UserRole>(req.Role, true, out var role)) return BadRequest("Invalid role");

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
        await _ctx.SaveChangesAsync();

        return Ok(new { user.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserRequest req)
    {
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null || actor.BusinessId != req.BusinessId) return Forbid();
        if (!(actor.Role == UserRole.ADMIN || actor.Role == UserRole.SUPERVISOR)) return Forbid();

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId);
        if (user == null) return NotFound();

        if (!Enum.TryParse<UserRole>(req.Role, true, out var newRole)) return BadRequest("Invalid role");

        // Supervisor should not modify ADMIN role (recommendation)
        if (actor.Role == UserRole.SUPERVISOR && user.Role == UserRole.ADMIN && newRole != UserRole.ADMIN)
            return Forbid();

        // prevent disabling last admin
        if (!req.IsActive && user.Role == UserRole.ADMIN)
        {
            var otherAdmins = await _ctx.Users.CountAsync(u => u.BusinessId == req.BusinessId && u.Role == UserRole.ADMIN && u.IsActive && u.Id != user.Id);
            if (otherAdmins == 0) return BadRequest("Cannot deactivate the last active admin");
        }

        user.Role = newRole;
        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _ctx.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword([FromRoute] Guid id, [FromBody] ResetPasswordRequest req)
    {
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null || actor.BusinessId != req.BusinessId) return Forbid();
        if (!(actor.Role == UserRole.ADMIN || actor.Role == UserRole.SUPERVISOR)) return Forbid();

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId);
        if (user == null) return NotFound();

        user.PasswordHash = _hasher.Hash(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/reset-pin")]
    public async Task<IActionResult> ResetPin([FromRoute] Guid id, [FromBody] ResetPinRequest req)
    {
        var actor = await _ctx.Users.FindAsync(req.ActorUserId);
        if (actor == null || actor.BusinessId != req.BusinessId) return Forbid();
        if (!(actor.Role == UserRole.ADMIN || actor.Role == UserRole.SUPERVISOR)) return Forbid();

        if (string.IsNullOrEmpty(req.NewPin4) || req.NewPin4.Length != 4 || !req.NewPin4.All(char.IsDigit)) return BadRequest("Pin must be exactly 4 digits");

        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id && u.BusinessId == req.BusinessId);
        if (user == null) return NotFound();

        user.PinHash = _hasher.Hash(req.NewPin4);
        user.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok();
    }
}
