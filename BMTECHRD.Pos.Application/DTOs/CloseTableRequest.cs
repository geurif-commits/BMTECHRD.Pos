namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CloseTableRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid TableId { get; set; }
    public required System.Guid ActorUserId { get; set; }
}
