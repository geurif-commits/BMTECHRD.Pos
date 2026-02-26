namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreatePaymentResponse
{
    public required decimal Paid { get; set; }
    public required decimal Due { get; set; }
    public required decimal Change { get; set; }
    public required bool Closed { get; set; }
    public string? CloseBlockedReason { get; set; }
}
