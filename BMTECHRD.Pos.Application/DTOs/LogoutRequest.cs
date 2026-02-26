namespace BMTECHRD.Pos.Application.DTOs;

public sealed class LogoutRequest
{
    public required string RefreshToken { get; set; }
}