namespace BMTECHRD.Pos.Application.DTOs;

public sealed class LoginResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }

    public required Guid UserId { get; set; }
    public required Guid BusinessId { get; set; }

    public required string Username { get; set; }
    public required string Role { get; set; }

    public required DateTime ExpiresAt { get; set; }
}