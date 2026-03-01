using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Business : AuditableEntity
{
    public required string Name { get; set; }

    // Logo: guardar ruta relativa: /uploads/logos/{file}.png
    public string? LogoPath { get; set; }

    public string? CurrencyCode { get; set; } = "DOP";

    // Impuestos y propina configurables
    public bool EnableItbis { get; set; } = true;
    public decimal ItbisRate { get; set; } = 0.18m;

    public bool EnableTip { get; set; } = true;
    public decimal TipRate { get; set; } = 0.10m;

    // Hoja de ruta fiscal RD
    public bool EnableFiscalReceipt { get; set; }
    public bool EnableElectronicInvoice { get; set; }

    public License? License { get; set; }
}
