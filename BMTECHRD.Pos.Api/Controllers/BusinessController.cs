using BMTECHRD.Pos.Api.Services.Business;
using BMTECHRD.Pos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Controllers;

[ApiController]
[Route("api/business")]
public sealed class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateBusinessRequest request, CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        var logo = form.Files.GetFile("logo");

        var response = await _businessService.CreateAsync(request, logo, ct);
        return CreatedAtAction(null, response);
    }

    [HttpPost("simple")]
    public async Task<IActionResult> CreateSimple([FromBody] CreateBusinessRequest request, CancellationToken ct)
    {
        var response = await _businessService.CreateAsync(request, null, ct);
        return CreatedAtAction(null, response);
    }
}
