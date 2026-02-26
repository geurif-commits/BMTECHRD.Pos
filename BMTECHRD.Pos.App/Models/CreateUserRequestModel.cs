using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class CreateUserRequestModel
{
    public Guid BusinessId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Pin4 { get; set; }
    public string Role { get; set; } = "WAITER";
    public bool IsActive { get; set; } = true;
}
