namespace BMTECHRD.Pos.Application.DTOs;
public sealed class TableAccessResponse
{
    public bool RequiresPin { get; set; }
    public bool AccessGranted { get; set; }
    public string? Reason { get; set; }
}
