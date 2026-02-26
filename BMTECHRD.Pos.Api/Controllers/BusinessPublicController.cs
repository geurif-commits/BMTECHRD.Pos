using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/business")]
public sealed class BusinessPublicController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public BusinessPublicController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic()
    {
        var list = await _ctx.Businesses
            .Select(b => new BusinessPublicDto { BusinessId = b.Id, Name = b.Name, LogoPath = b.LogoPath })
            .ToListAsync();
        return Ok(list);
    }
}
