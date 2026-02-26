namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateOrderBatchRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid TableId { get; set; }
    public required System.Guid ActorUserId { get; set; }
    public required System.Collections.Generic.List<CreateOrderBatchLine> Items { get; set; }
}
