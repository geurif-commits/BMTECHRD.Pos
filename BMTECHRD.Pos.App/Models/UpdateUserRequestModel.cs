using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class UpdateUserRequestModel
{
    public Guid BusinessId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Role { get; set; } = "WAITER";
    public bool IsActive { get; set; }
}
