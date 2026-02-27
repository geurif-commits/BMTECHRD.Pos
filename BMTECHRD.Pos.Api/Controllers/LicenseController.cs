using BMTECHRD.Pos.Api.Services.License;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/license")]
public sealed class LicenseController : ControllerBase
{
    private readonly ILicenseActivationService _licenseService;

    public LicenseController(ILicenseActivationService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request, CancellationToken ct)
    {
        var response = await _licenseService.ActivateAsync(request, ct);
        return Ok(response);
    }
}
