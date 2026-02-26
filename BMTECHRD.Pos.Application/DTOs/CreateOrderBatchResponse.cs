namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateOrderBatchResponse
{
    public required System.Guid OrderId { get; set; }
    public required int TotalItems { get; set; }
    public required int KitchenItems { get; set; }
    public required int BarItems { get; set; }
}
