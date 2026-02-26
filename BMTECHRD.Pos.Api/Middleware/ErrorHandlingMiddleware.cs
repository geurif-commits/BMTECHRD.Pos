using System.Net.Mime;
using System.Text.Json;
namespace BMTECHRD.Pos.Api.Middleware;
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            var payload = JsonSerializer.Serialize(new { error = "Unexpected error", detail = ex.Message });
            await context.Response.WriteAsync(payload);
        }
    }
}