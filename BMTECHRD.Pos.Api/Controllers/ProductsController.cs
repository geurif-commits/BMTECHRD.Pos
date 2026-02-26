using BMTECHRD.Pos.Infrastructure.Persistence;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProductsController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId, [FromQuery] Guid? categoryId, [FromQuery] string? q, [FromQuery] bool? active)
    {
        var query = _ctx.Products.AsQueryable().Where(p => p.BusinessId == businessId);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q));
        if (active.HasValue) query = query.Where(p => p.IsActive == active.Value);
        var list = await query.OrderBy(p => p.Name).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120) return BadRequest("Invalid name");
        if (dto.Price < 0) return BadRequest("Price must be >= 0");
        if (dto.Stock < 0) return BadRequest("Stock must be >= 0");
        // ensure unique name per business
        var exists = await _ctx.Products.AnyAsync(p => p.BusinessId == dto.BusinessId && p.Name == dto.Name);
        if (exists) return BadRequest("Product with same name already exists");
        // ensure category exists
        var cat = await _ctx.Categories.FindAsync(dto.CategoryId);
        if (cat == null) return BadRequest("Category not found");

        dto.Id = Guid.NewGuid();
        dto.CreatedAt = DateTime.UtcNow;
        _ctx.Products.Add(dto);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(null, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody] Product dto)
    {
        var prod = await _ctx.Products.FindAsync(id);
        if (prod == null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120) return BadRequest("Invalid name");
        if (dto.Price < 0) return BadRequest("Price must be >= 0");
        if (dto.Stock < 0) return BadRequest("Stock must be >= 0");
        var exists = await _ctx.Products.AnyAsync(p => p.BusinessId == prod.BusinessId && p.Name == dto.Name && p.Id != id);
        if (exists) return BadRequest("Product with same name already exists");
        prod.Name = dto.Name;
        prod.Price = dto.Price;
        prod.Stock = dto.Stock;
        prod.TrackInventory = dto.TrackInventory;
        prod.Area = dto.Area;
        prod.ImageUrl = dto.ImageUrl;
        prod.IsActive = dto.IsActive;
        prod.CategoryId = dto.CategoryId;
        prod.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(prod);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle([FromRoute] Guid id)
    {
        var prod = await _ctx.Products.FindAsync(id);
        if (prod == null) return NotFound();
        prod.IsActive = !prod.IsActive;
        prod.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok(prod);
    }
}
