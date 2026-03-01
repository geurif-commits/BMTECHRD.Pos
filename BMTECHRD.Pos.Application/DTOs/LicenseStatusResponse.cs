namespace BMTECHRD.Pos.Application.DTOs;

public sealed class LicenseStatusResponse
{
    public required System.Guid BusinessId { get; set; }
    public required string Status { get; set; }
    public required string Plan { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int? DaysRemaining { get; set; }
    public bool IsExpiringSoon { get; set; }
}
