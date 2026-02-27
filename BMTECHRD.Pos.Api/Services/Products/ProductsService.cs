using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Products;

public sealed class ProductsService : IProductsService
{
    private readonly AppDbContext _ctx;

    public ProductsService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<Product>> GetAsync(Guid businessId, Guid? categoryId, string? q, bool? active, CancellationToken ct)
    {
        var query = _ctx.Products.AsQueryable().Where(p => p.BusinessId == businessId);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q));
        if (active.HasValue) query = query.Where(p => p.IsActive == active.Value);
        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product> CreateAsync(Product dto, CancellationToken ct)
    {
        ValidateProduct(dto);

        var exists = await _ctx.Products.AnyAsync(p => p.BusinessId == dto.BusinessId && p.Name == dto.Name, ct);
        if (exists)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Duplicate product", "Product with same name already exists", "PROD_DUPLICATE_NAME");

        var cat = await _ctx.Categories.FindAsync(new object?[] { dto.CategoryId }, ct);
        if (cat == null)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Category invalid", "Category not found", "PROD_CATEGORY_NOT_FOUND");

        dto.Id = Guid.NewGuid();
        dto.CreatedAt = DateTime.UtcNow;
        _ctx.Products.Add(dto);
        await _ctx.SaveChangesAsync(ct);

        return dto;
    }

    public async Task<Product> EditAsync(Guid id, Product dto, CancellationToken ct)
    {
        var prod = await _ctx.Products.FindAsync(new object?[] { id }, ct);
        if (prod == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Product not found", "Product not found", "PROD_NOT_FOUND");

        ValidateProduct(dto);

        var exists = await _ctx.Products.AnyAsync(p => p.BusinessId == prod.BusinessId && p.Name == dto.Name && p.Id != id, ct);
        if (exists)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Duplicate product", "Product with same name already exists", "PROD_DUPLICATE_NAME");

        prod.Name = dto.Name;
        prod.Price = dto.Price;
        prod.Stock = dto.Stock;
        prod.TrackInventory = dto.TrackInventory;
        prod.Area = dto.Area;
        prod.ImageUrl = dto.ImageUrl;
        prod.IsActive = dto.IsActive;
        prod.CategoryId = dto.CategoryId;
        prod.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        return prod;
    }

    public async Task<Product> ToggleAsync(Guid id, CancellationToken ct)
    {
        var prod = await _ctx.Products.FindAsync(new object?[] { id }, ct);
        if (prod == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Product not found", "Product not found", "PROD_NOT_FOUND");

        prod.IsActive = !prod.IsActive;
        prod.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        return prod;
    }

    private static void ValidateProduct(Product dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid name", "Invalid name", "PROD_NAME_INVALID");
        if (dto.Price < 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid price", "Price must be >= 0", "PROD_PRICE_INVALID");
        if (dto.Stock < 0)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid stock", "Stock must be >= 0", "PROD_STOCK_INVALID");
    }
}
