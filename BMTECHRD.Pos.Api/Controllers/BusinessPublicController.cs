using BMTECHRD.Pos.Api.Services.Business;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/business")]
public sealed class BusinessPublicController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessPublicController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken ct)
    {
        var list = await _businessService.GetPublicAsync(ct);
        return Ok(list);
    }
}
