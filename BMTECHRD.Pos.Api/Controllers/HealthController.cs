using Microsoft.AspNetCore.Mvc;
namespace BMTECHRD.Pos.Api.Controllers;
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}