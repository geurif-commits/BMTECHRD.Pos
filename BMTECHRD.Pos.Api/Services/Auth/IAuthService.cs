using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest req, string? ipAddress, CancellationToken ct);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest req, string? ipAddress, CancellationToken ct);
    Task LogoutAsync(LogoutRequest req, string? subjectUserId, CancellationToken ct);
    (Guid UserId, Guid BusinessId, string? Role, string? Username) Me(System.Security.Claims.ClaimsPrincipal user);
}
