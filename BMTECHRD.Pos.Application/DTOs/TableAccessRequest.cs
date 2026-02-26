namespace BMTECHRD.Pos.Application.DTOs;
public sealed class TableAccessRequest
{
    public required Guid ActorUserId { get; set; }
    public required string Pin { get; set; }
}
