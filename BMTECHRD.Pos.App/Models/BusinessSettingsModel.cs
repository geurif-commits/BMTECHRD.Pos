namespace BMTECHRD.Pos.App.Models;

public sealed class BusinessSettingsModel
{
    public System.Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public bool EnableItbis { get; set; }
    public decimal ItbisRate { get; set; }
    public bool EnableTip { get; set; }
    public decimal TipRate { get; set; }
    public bool EnableFiscalReceipt { get; set; }
    public bool EnableElectronicInvoice { get; set; }
}
