namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreateShiftResponse
{
    public required System.Guid ShiftId { get; set; }
    public required System.DateTime OpenedAt { get; set; }
}
