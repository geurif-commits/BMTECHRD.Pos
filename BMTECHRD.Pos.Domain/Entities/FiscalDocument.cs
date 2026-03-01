using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class FiscalDocument : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Guid TableId { get; set; }
    public Guid? PaymentId { get; set; }

    // NFC o ECF
    public string Type { get; set; } = "NFC";
    public string Number { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Tip { get; set; }
    public decimal Total { get; set; }

    public string Status { get; set; } = "ISSUED";
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
