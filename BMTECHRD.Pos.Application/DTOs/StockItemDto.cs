namespace BMTECHRD.Pos.Application.DTOs;
public sealed class StockItemDto
{
    public required System.Guid ProductId { get; set; }
    public required string Name { get; set; }
    public required int Stock { get; set; }
    public required bool TrackInventory { get; set; }
}
