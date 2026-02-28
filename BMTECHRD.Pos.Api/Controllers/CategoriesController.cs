using BMTECHRD.Pos.Api.Services.Categories;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId, CancellationToken ct)
    {
        var list = await _categoriesService.GetAsync(businessId, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category dto, CancellationToken ct)
    {
        var created = await _categoriesService.CreateAsync(dto, ct);
        return CreatedAtAction(null, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody] Category dto, CancellationToken ct)
    {
        var cat = await _categoriesService.EditAsync(id, dto, ct);
        return Ok(cat);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle([FromRoute] Guid id, CancellationToken ct)
    {
        var cat = await _categoriesService.ToggleAsync(id, ct);
        return Ok(cat);
    }
}
