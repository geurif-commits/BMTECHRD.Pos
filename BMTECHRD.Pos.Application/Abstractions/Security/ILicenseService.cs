namespace BMTECHRD.Pos.Application.Abstractions.Security;

public interface ILicenseService
{
    Task<bool> IsBusinessActiveAsync(Guid businessId, CancellationToken ct);
}