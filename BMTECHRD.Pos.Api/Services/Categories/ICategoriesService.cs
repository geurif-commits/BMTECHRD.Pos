using BMTECHRD.Pos.Domain.Entities;

namespace BMTECHRD.Pos.Api.Services.Categories;

public interface ICategoriesService
{
    Task<List<Category>> GetAsync(Guid businessId, CancellationToken ct);
    Task<Category> CreateAsync(Category dto, CancellationToken ct);
    Task<Category> EditAsync(Guid id, Category dto, CancellationToken ct);
    Task<Category> ToggleAsync(Guid id, CancellationToken ct);
}
