namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ResetPasswordRequest
{
    public required System.Guid BusinessId { get; set; }
    public System.Guid? ActorUserId { get; set; }
    public required string NewPassword { get; set; }
}
