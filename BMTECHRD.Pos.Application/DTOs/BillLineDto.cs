namespace BMTECHRD.Pos.Application.DTOs;
public sealed class BillLineDto
{
    public required System.Guid ProductId { get; set; }
    public required string Name { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal LineTotal { get; set; }
    public required string Area { get; set; }
    public required string Status { get; set; }
}
