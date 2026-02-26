using BMTECHRD.Pos.Domain.Common;
using BMTECHRD.Pos.Domain.Enums;

namespace BMTECHRD.Pos.Domain.Entities;

public sealed class User : AuditableEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    // PIN 4 dígitos (hash)
    public string? PinHash { get; set; }

    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }
}