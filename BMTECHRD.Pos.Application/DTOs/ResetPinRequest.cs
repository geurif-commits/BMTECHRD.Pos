namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ResetPinRequest
{
    public required System.Guid BusinessId { get; set; }
    public System.Guid? ActorUserId { get; set; }
    public required string NewPin4 { get; set; }
}
