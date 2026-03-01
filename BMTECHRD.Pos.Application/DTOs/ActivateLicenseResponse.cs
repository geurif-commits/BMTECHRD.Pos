namespace BMTECHRD.Pos.Application.DTOs;

public sealed class ActivateLicenseResponse
{
    public required string Status { get; set; }
    public required string Plan { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int? DaysRemaining { get; set; }
}
