using BMTECHRD.Pos.Api.Middleware;
namespace BMTECHRD.Pos.Api.Extensions;
public static class DependencyInjection
{
    public static WebApplication UseApiDefaults(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        return app;
    }
}