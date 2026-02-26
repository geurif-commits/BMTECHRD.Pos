namespace BMTECHRD.Pos.App.Models;

public sealed class SessionModel
{
    public required System.Guid BusinessId { get; set; }
    public required System.Guid UserId { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public string AccessToken { get; set; } = string.Empty;  // Agregado BLOQUE 7
    public string RefreshToken { get; set; } = string.Empty; // Agregado BLOQUE 7
    public System.DateTime ExpiresAt { get; set; }            // Agregado BLOQUE 7
}
