using BMTECHRD.Pos.Domain.Common;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Business : AuditableEntity
{
    public required string Name { get; set; }

    // Logo: guardar ruta relativa: /uploads/logos/{file}.png
    public string? LogoPath { get; set; }

    // Configuración opcional
    public string? CurrencyCode { get; set; } = "DOP";

    // Relación 1-1 con licencia
    public License? License { get; set; }
}