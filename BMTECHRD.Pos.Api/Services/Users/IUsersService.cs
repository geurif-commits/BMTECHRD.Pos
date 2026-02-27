using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Users;

public interface IUsersService
{
    Task<List<UserListItemDto>> GetAsync(Guid businessId, CancellationToken ct);
    Task<Guid> CreateAsync(CreateUserRequest req, CancellationToken ct);
    Task UpdateAsync(Guid id, UpdateUserRequest req, CancellationToken ct);
    Task ResetPasswordAsync(Guid id, ResetPasswordRequest req, CancellationToken ct);
    Task ResetPinAsync(Guid id, ResetPinRequest req, CancellationToken ct);
}
