namespace BMTECHRD.Pos.App.Models;

public sealed class MenuProductModel
{
    public System.Guid Id { get; set; }
    public System.Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string Area { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
