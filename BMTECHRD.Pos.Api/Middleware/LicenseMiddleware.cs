using BMTECHRD.Pos.Application.Abstractions.Security;
using System.Security.Claims;

namespace BMTECHRD.Pos.Api.Middleware;

public sealed class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/api/health")
            || path.StartsWith("/api/license/activate")
            || path.StartsWith("/api/license/status")
            || path.StartsWith("/api/license/alerts")
            || path.StartsWith("/api/business/public")
            || path.StartsWith("/api/auth/login")
            || path.StartsWith("/api/auth/refresh")
            || path.StartsWith("/api/auth/logout")
            || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        string? businessIdStr = context.Request.Query["businessId"].FirstOrDefault();
        if (string.IsNullOrEmpty(businessIdStr) && context.Request.Headers.TryGetValue("X-Business-Id", out var vals))
            businessIdStr = vals.FirstOrDefault();

        if (string.IsNullOrEmpty(businessIdStr))
            businessIdStr = context.User.FindFirst("bid")?.Value ?? context.User.FindFirst(ClaimTypes.GroupSid)?.Value;

        if (string.IsNullOrEmpty(businessIdStr))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing businessId");
            return;
        }

        if (!Guid.TryParse(businessIdStr, out var businessId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid businessId");
            return;
        }

        var active = await licenseService.IsBusinessActiveAsync(businessId, context.RequestAborted);
        if (!active)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Business license is not active");
            return;
        }

        await _next(context);
    }
}
