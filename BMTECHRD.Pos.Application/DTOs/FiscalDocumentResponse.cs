namespace BMTECHRD.Pos.Application.DTOs;

public sealed class FiscalDocumentResponse
{
    public required System.Guid Id { get; set; }
    public required string Type { get; set; }
    public required string Number { get; set; }
    public required decimal Subtotal { get; set; }
    public required decimal Tax { get; set; }
    public required decimal Tip { get; set; }
    public required decimal Total { get; set; }
    public required System.DateTime IssuedAt { get; set; }
}
