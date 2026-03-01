namespace BMTECHRD.Pos.Application.DTOs;

public sealed class IssueFiscalDocumentRequest
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid TableId { get; set; }
    public string Type { get; set; } = "NFC";
}
