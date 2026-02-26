namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateShiftRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid UserId { get; set; }
    public required decimal OpeningCash { get; set; }
    public string? Notes { get; set; }
}
