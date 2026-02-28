using BMTECHRD.Pos.Domain.Entities;

namespace BMTECHRD.Pos.Api.Services.Products;

public interface IProductsService
{
    Task<List<Product>> GetAsync(Guid businessId, Guid? categoryId, string? q, bool? active, CancellationToken ct);
    Task<Product> CreateAsync(Product dto, CancellationToken ct);
    Task<Product> EditAsync(Guid id, Product dto, CancellationToken ct);
    Task<Product> ToggleAsync(Guid id, CancellationToken ct);
}
