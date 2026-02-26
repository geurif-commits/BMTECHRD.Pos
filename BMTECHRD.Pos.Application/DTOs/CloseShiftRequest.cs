namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CloseShiftRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid UserId { get; set; }
    public required System.Guid ShiftId { get; set; }
    public required decimal ClosingCash { get; set; }
    public string? Notes { get; set; }
}
