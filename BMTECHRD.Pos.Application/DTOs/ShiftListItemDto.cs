namespace BMTECHRD.Pos.Application.DTOs;
public sealed class ShiftListItemDto
{
    public required System.Guid ShiftId { get; set; }
    public required System.Guid UserId { get; set; }
    public required string Username { get; set; }
    public required System.DateTime OpenedAt { get; set; }
    public System.DateTime? ClosedAt { get; set; }
    public required decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public required string Status { get; set; }
}
