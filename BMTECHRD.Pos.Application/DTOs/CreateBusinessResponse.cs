namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateBusinessResponse
{
    public required Guid BusinessId { get; set; }
    public required string Name { get; set; }
    public string? LogoPath { get; set; }
    public required string ActivationKey { get; set; }
}
