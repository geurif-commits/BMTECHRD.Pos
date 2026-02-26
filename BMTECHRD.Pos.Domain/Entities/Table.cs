using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class Table : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }

    public int Number { get; set; }
    public TableStatus Status { get; set; } = TableStatus.AVAILABLE;

    // Mesa abierta / control de dueño
    public Guid? OpenedByWaiterId { get; set; }
    public User? OpenedByWaiter { get; set; }
    public DateTime? OpenedAt { get; set; }

    // Posición en el mapa (drag & drop)
    public double PosX { get; set; } = 0;
    public double PosY { get; set; } = 0;
}