using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Category : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }

    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product>? Products { get; set; }
}