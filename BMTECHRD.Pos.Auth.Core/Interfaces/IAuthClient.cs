using System.Threading;
using System.Threading.Tasks;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Auth.Core.Interfaces;

public interface IAuthClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken cancellationToken);
    Task<LoginResponse?> RefreshAsync(RefreshTokenRequest req, CancellationToken cancellationToken);
}