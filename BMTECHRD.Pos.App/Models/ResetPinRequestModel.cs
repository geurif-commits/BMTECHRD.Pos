using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class ResetPinRequestModel
{
    public Guid BusinessId { get; set; }
    public Guid ActorUserId { get; set; }
    public string NewPin4 { get; set; } = string.Empty;
}
