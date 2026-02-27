using BMTECHRD.Pos.Api.Services.Users;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId, CancellationToken ct)
    {
        var list = await _usersService.GetAsync(businessId, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var id = await _usersService.CreateAsync(req, ct);
        return Ok(new { Id = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        await _usersService.UpdateAsync(id, req, ct);
        return Ok();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword([FromRoute] Guid id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await _usersService.ResetPasswordAsync(id, req, ct);
        return Ok();
    }

    [HttpPost("{id}/reset-pin")]
    public async Task<IActionResult> ResetPin([FromRoute] Guid id, [FromBody] ResetPinRequest req, CancellationToken ct)
    {
        await _usersService.ResetPinAsync(id, req, ct);
        return Ok();
    }
}
