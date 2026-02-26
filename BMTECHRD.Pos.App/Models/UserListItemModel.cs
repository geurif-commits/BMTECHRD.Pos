using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class UserListItemModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool HasPin { get; set; }
    public DateTime CreatedAt { get; set; }
}
