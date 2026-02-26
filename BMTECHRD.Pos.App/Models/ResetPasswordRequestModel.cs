using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class ResetPasswordRequestModel
{
    public Guid BusinessId { get; set; }
    public Guid ActorUserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
