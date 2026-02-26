namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateUserRequest
{
    public required System.Guid BusinessId { get; set; }
    public System.Guid? ActorUserId { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Pin4 { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; } = true;
}
