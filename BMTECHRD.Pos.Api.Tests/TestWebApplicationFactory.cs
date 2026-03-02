using BMTECHRD.Pos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BMTECHRD.Pos.Api.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IServiceProvider? _inMemoryServiceProvider;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Test environment to prevent development seeding
        builder.UseEnvironment("Test");

        // Ensure process environment variable is set so AddInfrastructure can detect Test env if it reads env vars
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        builder.ConfigureTestServices(services =>
        {
            // Remove ALL DbContext and DbContextOptions registrations
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            // Register InMemoryDatabase
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });
        });
    }

    public void SeedDatabase()
    {
        // Build a standalone in-memory DbContext for seeding to avoid DI provider conflicts
        var inMemoryServiceProvider = _inMemoryServiceProvider ?? new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("IntegrationTestDb")
            .UseInternalServiceProvider(inMemoryServiceProvider)
            .Options;

        using var db = new AppDbContext(options);
        var hasher = new BMTECHRD.Pos.Infrastructure.Auth.PasswordHasher();

        db.Database.EnsureCreated();

        if (!db.Businesses.Any())
        {
            var business = new BMTECHRD.Pos.Domain.Entities.Business
            {
                Name = "TEST BUSINESS",
                CurrencyCode = "USD"
            };

            var license = new BMTECHRD.Pos.Domain.Entities.License
            {
                ActivationKey = "TEST-KEY",
                Plan = BMTECHRD.Pos.Domain.Enums.LicensePlan.LIFETIME,
                Status = BMTECHRD.Pos.Domain.Enums.LicenseStatus.ACTIVE,
                Business = business
            };
            business.License = license;

            var admin = new BMTECHRD.Pos.Domain.Entities.User
            {
                Username = "admin",
                PasswordHash = hasher.Hash("admin"),
                PinHash = hasher.Hash("1234"),
                Role = BMTECHRD.Pos.Domain.Enums.UserRole.ADMIN,
                Business = business
            };

            db.Businesses.Add(business);
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}
