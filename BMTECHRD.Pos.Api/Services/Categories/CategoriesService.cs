using BMTECHRD.Pos.Api.Common;
using BMTECHRD.Pos.Domain.Entities;
using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BMTECHRD.Pos.Api.Services.Categories;

public sealed class CategoriesService : ICategoriesService
{
    private readonly AppDbContext _ctx;

    public CategoriesService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<List<Category>> GetAsync(Guid businessId, CancellationToken ct)
        => _ctx.Categories.AsNoTracking().Where(c => c.BusinessId == businessId).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);

    public async Task<Category> CreateAsync(Category dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 60)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid name", "Invalid name", "CAT_NAME_INVALID");

        var exists = await _ctx.Categories.AnyAsync(c => c.BusinessId == dto.BusinessId && c.Name == dto.Name, ct);
        if (exists)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Duplicate category", "Category with same name already exists", "CAT_DUPLICATE_NAME");

        dto.Id = Guid.NewGuid();
        dto.CreatedAt = DateTime.UtcNow;
        _ctx.Categories.Add(dto);
        await _ctx.SaveChangesAsync(ct);

        return dto;
    }

    public async Task<Category> EditAsync(Guid id, Category dto, CancellationToken ct)
    {
        var cat = await _ctx.Categories.FindAsync(new object?[] { id }, ct);
        if (cat == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Category not found", "Category not found", "CAT_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 60)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Invalid name", "Invalid name", "CAT_NAME_INVALID");

        var exists = await _ctx.Categories.AnyAsync(c => c.BusinessId == cat.BusinessId && c.Name == dto.Name && c.Id != id, ct);
        if (exists)
            throw new ApiProblemException(StatusCodes.Status400BadRequest, "Duplicate category", "Category with same name already exists", "CAT_DUPLICATE_NAME");

        cat.Name = dto.Name;
        cat.SortOrder = dto.SortOrder;
        cat.IsActive = dto.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        return cat;
    }

    public async Task<Category> ToggleAsync(Guid id, CancellationToken ct)
    {
        var cat = await _ctx.Categories.FindAsync(new object?[] { id }, ct);
        if (cat == null)
            throw new ApiProblemException(StatusCodes.Status404NotFound, "Category not found", "Category not found", "CAT_NOT_FOUND");

        cat.IsActive = !cat.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);

        return cat;
    }
}
