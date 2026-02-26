namespace BMTECHRD.Pos.Application.DTOs;
public sealed class UserListItemDto
{
    public required System.Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public required bool IsActive { get; set; }
    public required bool HasPin { get; set; }
    public required System.DateTime CreatedAt { get; set; }
}
