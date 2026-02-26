using BMTECHRD.Pos.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace BMTECHRD.Pos.Api.Middleware;

public sealed class LicenseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILicenseService _licenseService;

    public LicenseMiddleware(RequestDelegate next, ILicenseService licenseService)
    {
        _next = next;
        _licenseService = licenseService;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        // allow health, license activation, public business list, auth login and swagger
        if (path.StartsWith("/api/health") || path.StartsWith("/api/license/activate") || path.StartsWith("/api/business/public") || path.StartsWith("/api/auth/login") || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        // Try to get businessId from query or header
        string? businessIdStr = context.Request.Query["businessId"].FirstOrDefault();
        if (string.IsNullOrEmpty(businessIdStr))
        {
            if (context.Request.Headers.TryGetValue("X-Business-Id", out var vals))
                businessIdStr = vals.FirstOrDefault();
        }

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

        var active = await _licenseService.IsBusinessActiveAsync(businessId, context.RequestAborted);
        if (!active)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Business license is not active");
            return;
        }

        await _next(context);
    }
}
