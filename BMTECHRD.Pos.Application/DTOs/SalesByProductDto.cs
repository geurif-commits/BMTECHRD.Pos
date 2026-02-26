namespace BMTECHRD.Pos.Application.DTOs;
public sealed class SalesByProductDto
{
    public required System.Guid ProductId { get; set; }
    public required string Name { get; set; }
    public required int Qty { get; set; }
    public required decimal Total { get; set; }
}
