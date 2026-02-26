namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateOrderBatchLine
{
    public required System.Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
