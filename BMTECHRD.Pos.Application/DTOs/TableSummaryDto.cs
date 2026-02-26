namespace BMTECHRD.Pos.Application.DTOs;
public sealed class TableSummaryDto
{
    public required System.Guid TableId { get; set; }
    public required int TableNumber { get; set; }
    public required string Status { get; set; }
    public required decimal CurrentTotal { get; set; }
    public required int ItemsCount { get; set; }
    public required bool HasPendingItems { get; set; }
}
