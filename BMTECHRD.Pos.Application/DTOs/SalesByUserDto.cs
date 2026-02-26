namespace BMTECHRD.Pos.Application.DTOs;
public sealed class SalesByUserDto
{
    public required System.Guid UserId { get; set; }
    public required string Username { get; set; }
    public required decimal Total { get; set; }
}
