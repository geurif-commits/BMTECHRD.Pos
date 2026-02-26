using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class ShiftListItemModel
{
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public string Status { get; set; } = string.Empty;

    public string StatusLabel => Status switch
    {
        "OPEN" => "ABIERTO",
        "CLOSED" => "CERRADO",
        _ => Status
    };

    public bool IsOpen => Status == "OPEN";
    public bool IsClosed => Status == "CLOSED";
    public string OpenedAtLabel => OpenedAt.ToString("g");
    public string ClosedAtLabel => ClosedAt?.ToString("g") ?? "-";
}
