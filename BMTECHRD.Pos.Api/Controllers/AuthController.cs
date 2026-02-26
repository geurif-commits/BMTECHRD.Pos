using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext ctx, IPasswordHasher hasher, ITokenService tokenService)
    {
        _ctx = ctx;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var business = await _ctx.Businesses.FindAsync(req.BusinessId);
        if (business == null)
            return NotFound("Business not found");

        var user = await _ctx.Users.FirstOrDefaultAsync(
            u => u.BusinessId == req.BusinessId && u.Username == req.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var ok = _hasher.Verify(req.Password, user.PasswordHash ?? string.Empty);
        if (!ok)
            return Unauthorized("Invalid credentials");

        var accessToken = _tokenService.CreateAccessToken(user, req.BusinessId);

        var (refreshTokenRaw, refreshTokenEntity) = await _tokenService.CreateRefreshTokenAsync(
            user,
            req.BusinessId,
            deviceId: req.DeviceId,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = req.BusinessId,
            ActorUserId = user.Id,
            Action = "AUTH_LOGIN",
            EntityType = "User",
            EntityId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenRaw,
            UserId = user.Id,
            BusinessId = req.BusinessId,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = refreshTokenEntity.ExpiresAt
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return BadRequest("Refresh token is required");

        var validation = await _tokenService.ValidateRefreshTokenDetailedAsync(req.RefreshToken, req.DeviceId);

        if (validation.IsIncident)
        {
            // Auditoría incidente
            _ctx.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                BusinessId = Guid.Empty,
                ActorUserId = Guid.Empty,
                Action = validation.Reason == "DEVICE_MISMATCH"
                    ? "AUTH_REFRESH_DEVICE_MISMATCH"
                    : "AUTH_REFRESH_REUSE_DETECTED",
                EntityType = "RefreshToken",
                EntityId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            });

            await _ctx.SaveChangesAsync();

            return Unauthorized(validation.Reason == "DEVICE_MISMATCH"
                ? "Device mismatch. Please login again."
                : "Refresh token reuse detected. Please login again.");
        }

        if (!validation.IsValid || validation.Token == null)
            return Unauthorized("Invalid or expired refresh token");

        var refreshToken = validation.Token;

        var user = await _ctx.Users.FindAsync(refreshToken.UserId);
        if (user == null)
            return Unauthorized("User not found");

        var newAccessToken = _tokenService.CreateAccessToken(user, refreshToken.BusinessId);

        var (newRefreshTokenRaw, newRefreshTokenEntity) = await _tokenService.CreateRefreshTokenAsync(
            user,
            refreshToken.BusinessId,
            deviceId: req.DeviceId ?? refreshToken.DeviceId,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        await _tokenService.RevokeRefreshTokenAsync(refreshToken.Id, newRefreshTokenEntity.Id);

        _ctx.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            BusinessId = refreshToken.BusinessId,
            ActorUserId = user.Id,
            Action = "AUTH_REFRESH",
            EntityType = "User",
            EntityId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenRaw,
            UserId = user.Id,
            BusinessId = refreshToken.BusinessId,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = newRefreshTokenEntity.ExpiresAt
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return BadRequest("Refresh token is required");

        var refreshToken = await _tokenService.ValidateRefreshTokenAsync(req.RefreshToken);
        if (refreshToken != null)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken.Id);

            var sub = User.FindFirst("sub")?.Value;
            if (Guid.TryParse(sub, out var userIdGuid))
            {
                _ctx.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    BusinessId = refreshToken.BusinessId,
                    ActorUserId = userIdGuid,
                    Action = "AUTH_LOGOUT",
                    EntityType = "User",
                    EntityId = userIdGuid,
                    CreatedAt = DateTime.UtcNow
                });

                await _ctx.SaveChangesAsync();
            }
        }

        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst("sub")?.Value;
        var businessId = User.FindFirst("bid")?.Value;
        var role = User.FindFirst("role")?.Value;
        var username = User.FindFirst("uname")?.Value;

        if (!Guid.TryParse(userId, out var userIdGuid) || !Guid.TryParse(businessId, out var businessIdGuid))
            return Unauthorized();

        return Ok(new
        {
            userId = userIdGuid,
            businessId = businessIdGuid,
            role,
            username
        });
    }
}