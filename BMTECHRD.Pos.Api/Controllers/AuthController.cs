using BMTECHRD.Pos.Api.Services.Auth;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, Microsoft.Extensions.Logging.ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var resp = await _authService.LoginAsync(req, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(resp);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var resp = await _authService.RefreshAsync(req, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(resp);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        await _authService.LogoutAsync(req, User.FindFirst("sub")?.Value, ct);
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var data = _authService.Me(User);
        return Ok(new
        {
            userId = data.UserId,
            businessId = data.BusinessId,
            role = data.Role,
            username = data.Username
        });
    }
}
