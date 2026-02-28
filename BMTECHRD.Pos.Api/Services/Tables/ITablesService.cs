using BMTECHRD.Pos.Application.DTOs;
using BMTECHRD.Pos.Domain.Entities;

namespace BMTECHRD.Pos.Api.Services.Tables;

public interface ITablesService
{
    Task<List<Table>> GetAsync(Guid businessId, CancellationToken ct);
    Task<Table> OpenAsync(Guid id, OpenTableRequest req, CancellationToken ct);
    Task<TableAccessResponse> AccessAsync(Guid id, TableAccessRequest req, CancellationToken ct);
    Task<Table> UpdatePositionAsync(Guid id, UpdateTablePositionRequest req, CancellationToken ct);
}
