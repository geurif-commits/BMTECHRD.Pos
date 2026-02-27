using BMTECHRD.Pos.Api.Services.Products;
using BMTECHRD.Pos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid businessId, [FromQuery] Guid? categoryId, [FromQuery] string? q, [FromQuery] bool? active, CancellationToken ct)
    {
        var list = await _productsService.GetAsync(businessId, categoryId, q, active, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product dto, CancellationToken ct)
    {
        var created = await _productsService.CreateAsync(dto, ct);
        return CreatedAtAction(null, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody] Product dto, CancellationToken ct)
    {
        var prod = await _productsService.EditAsync(id, dto, ct);
        return Ok(prod);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle([FromRoute] Guid id, CancellationToken ct)
    {
        var prod = await _productsService.ToggleAsync(id, ct);
        return Ok(prod);
    }
}
