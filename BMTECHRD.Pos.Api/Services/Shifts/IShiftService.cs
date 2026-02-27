using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.Shifts;

public interface IShiftService
{
    Task<ShiftStatusResponse> GetActiveAsync(Guid businessId, Guid userId, CancellationToken ct);
    Task<List<ShiftListItemDto>> ListAsync(Guid businessId, Guid? userId, DateTime? from, DateTime? to, string? status, int? limit, Guid? actorUserId, CancellationToken ct);
    Task<CreateShiftResponse> OpenAsync(CreateShiftRequest req, string? idempotencyKey, CancellationToken ct);
    Task<ShiftSummaryDto> CloseAsync(CloseShiftRequest req, string? idempotencyKey, CancellationToken ct);
    Task<ShiftSummaryDto> SummaryAsync(Guid shiftId, Guid businessId, CancellationToken ct);
}
