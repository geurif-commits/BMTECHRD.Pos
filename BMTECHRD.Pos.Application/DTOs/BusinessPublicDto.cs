namespace BMTECHRD.Pos.Application.DTOs;
public sealed class BusinessPublicDto
{
    public required System.Guid BusinessId { get; set; }
    public required string Name { get; set; }
    public string? LogoPath { get; set; }
}
