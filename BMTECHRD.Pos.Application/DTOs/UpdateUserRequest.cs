namespace BMTECHRD.Pos.Application.DTOs;
public sealed class UpdateUserRequest
{
    public required System.Guid BusinessId { get; set; }
    public System.Guid? ActorUserId { get; set; }
    public required string Role { get; set; }
    public required bool IsActive { get; set; }
}
