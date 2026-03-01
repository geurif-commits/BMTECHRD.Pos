namespace BMTECHRD.Pos.App.Models;

public sealed class MenuCategoryModel
{
    public System.Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
