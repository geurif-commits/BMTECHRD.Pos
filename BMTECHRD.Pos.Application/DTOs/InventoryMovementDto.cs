namespace BMTECHRD.Pos.Application.DTOs;
public sealed class InventoryMovementDto
{
    public required System.Guid Id { get; set; }
    public required System.Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public required int QuantityDelta { get; set; }
    public required string Reason { get; set; }
    public System.Guid? ActorUserId { get; set; }
    public string? ActorUsername { get; set; }
    public required System.DateTime CreatedAt { get; set; }
}
