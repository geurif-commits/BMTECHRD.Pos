namespace BMTECHRD.Pos.Application.DTOs;

public sealed class UpdateBusinessSettingsRequest
{
    public required string Name { get; set; }
    public bool EnableItbis { get; set; }
    public decimal ItbisRate { get; set; }
    public bool EnableTip { get; set; }
    public decimal TipRate { get; set; }
    public bool EnableFiscalReceipt { get; set; }
    public bool EnableElectronicInvoice { get; set; }
}
