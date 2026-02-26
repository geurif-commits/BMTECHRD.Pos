namespace BMTECHRD.Pos.Application.DTOs;
public sealed class CreatePaymentRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid TableId { get; set; }
    public required System.Guid ShiftId { get; set; }
    public required System.Guid ActorUserId { get; set; }
    public required string Method { get; set; }
    public required decimal Amount { get; set; }
    public object? Meta { get; set; }
    public decimal? CashGiven { get; set; }
    public bool CloseIfPaid { get; set; } = true;
}
