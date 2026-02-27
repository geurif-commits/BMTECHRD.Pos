using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.Api.Services.License;

public interface ILicenseActivationService
{
    Task<ActivateLicenseResponse> ActivateAsync(ActivateLicenseRequest request, CancellationToken ct);
}
