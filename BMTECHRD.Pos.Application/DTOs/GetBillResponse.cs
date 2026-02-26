namespace BMTECHRD.Pos.Application.DTOs;
public sealed class GetBillResponse
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid TableId { get; set; }
    public required int TableNumber { get; set; }
    public required System.Collections.Generic.List<BillLineDto> Lines { get; set; }
    public required decimal Subtotal { get; set; }
    public required decimal Tax { get; set; }
    public required decimal Tip { get; set; }
    public required decimal Discount { get; set; }
    public required decimal Total { get; set; }
    public required decimal Paid { get; set; }
    public required decimal Due { get; set; }
    public required bool HasPendingItems { get; set; }
}
