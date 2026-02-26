namespace BMTECHRD.Pos.Application.DTOs;
public sealed class InventoryAdjustRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid ProductId { get; set; }
    public required int QuantityDelta { get; set; }
    public required string Reason { get; set; }
    public System.Guid? ActorUserId { get; set; }
}
