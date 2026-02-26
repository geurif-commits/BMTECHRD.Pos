using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BMTECHRD.Pos.Application.Abstractions.Security;
using BMTECHRD.Pos.Infrastructure.Auth;

namespace BMTECHRD.Pos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

        // Auth and policies
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<ITableAccessPolicy, TableAccessPolicy>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
