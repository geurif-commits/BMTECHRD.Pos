namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ProductionQueueItemDto
{
    public required System.Guid OrderItemId { get; set; }
    public required System.Guid OrderId { get; set; }
    public required System.Guid TableId { get; set; }
    public required int TableNumber { get; set; }
    public required string ProductName { get; set; }
    public required int Quantity { get; set; }
    public required string Status { get; set; }
    public required string Area { get; set; }
    public required System.DateTime CreatedAt { get; set; }
}