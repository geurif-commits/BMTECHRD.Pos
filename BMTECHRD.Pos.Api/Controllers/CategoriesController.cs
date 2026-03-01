using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Authorize(Policy = "SupervisorOrAdmin")]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public CategoriesController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId)
    {
        var list = await _ctx.Categories.Where(c => c.BusinessId == businessId).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 60) return BadRequest("Invalid name");
        dto.Id = Guid.NewGuid();
        dto.CreatedAt = DateTime.UtcNow;
        // ensure unique name per business
        var exists = await _ctx.Categories.AnyAsync(c => c.BusinessId == dto.BusinessId && c.Name == dto.Name);
        if (exists) return BadRequest("Category with same name already exists");
        _ctx.Categories.Add(dto);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(null, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody] Category dto)
    {
        var cat = await _ctx.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 60) return BadRequest("Invalid name");
        var exists = await _ctx.Categories.AnyAsync(c => c.BusinessId == cat.BusinessId && c.Name == dto.Name && c.Id != id);
        if (exists) return BadRequest("Category with same name already exists");
        cat.Name = dto.Name;
        cat.SortOrder = dto.SortOrder;
        cat.IsActive = dto.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle([FromRoute] Guid id)
    {
        var cat = await _ctx.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        cat.IsActive = !cat.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(cat);
    }
}
