namespace BMTECHRD.Pos.Application.DTOs;

public sealed class RefreshTokenRequest
{
    public required string RefreshToken { get; set; }
    public string? DeviceId { get; set; }
}