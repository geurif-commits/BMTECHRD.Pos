namespace BMTECHRD.Pos.Application.DTOs;

public sealed class LoginRequest
{
    public required System.Guid BusinessId { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? DeviceId { get; set; }
}