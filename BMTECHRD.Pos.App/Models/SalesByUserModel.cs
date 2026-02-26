using System;

namespace BMTECHRD.Pos.App.Models;

public sealed class SalesByUserModel
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
