namespace BMTECHRD.Pos.App.Models;

public sealed class BusinessPublicModel
{
    public required System.Guid BusinessId { get; set; }
    public required string Name { get; set; }
    public string? LogoPath { get; set; }
}
