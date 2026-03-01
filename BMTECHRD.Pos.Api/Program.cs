using BMTECHRD.Pos.Api.Services.Orders;
using BMTECHRD.Pos.Api.Services.Reports;
using BMTECHRD.Pos.Api.Services.Inventory;
using BMTECHRD.Pos.Api.Services.Shifts;
using BMTECHRD.Pos.Api.Services.Cashier;
using BMTECHRD.Pos.Api.Services.Products;
using BMTECHRD.Pos.Api.Services.Tables;
using BMTECHRD.Pos.Api.Services.Users;
using BMTECHRD.Pos.Api.Services.Business;
using BMTECHRD.Pos.Api.Services.InventoryMovements;
using BMTECHRD.Pos.Api.Services.License;
using BMTECHRD.Pos.Api.Services.Categories;
using BMTECHRD.Pos.Api.Services.Auth;
using BMTECHRD.Pos.Api.Services.Idempotency;
using BMTECHRD.Pos.Api.Services.Production;
using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Extensions;
using BMTECHRD.Pos.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Capture model validation failures and log them to assist integration test diagnostics
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
        try
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value.Errors.Count > 0)
                .Select(kvp => new { Key = kvp.Key, Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                .ToArray();

            logger?.LogInformation("[ModelValidation] Invalid model state for {Path}: {Errors}", context.HttpContext.Request.Path, System.Text.Json.JsonSerializer.Serialize(errors));
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "[ModelValidation] Failed to log model state");
        }

        return new BadRequestObjectResult(context.ModelState);
    };
});
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderBatchService, OrderBatchService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICashierService, CashierService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ITablesService, TablesService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<ILicenseActivationService, LicenseActivationService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IInventoryMovementsService, InventoryMovementsService>();
builder.Services.AddScoped<IIdempotencyKeyStore, AuditLogIdempotencyKeyStore>();
builder.Services.AddScoped<IProductionQueueService, ProductionQueueService>();

// Infrastructure (DbContext, Auth services)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication (BLOQUE 4 + ETAPA 9 alignment)
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured");
var signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),

            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"] ?? "BMTECHRD.Pos.Api",

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"] ?? "BMTECHRD.Pos.App",

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            // ✅ ETAPA 9: claims alignment
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        // Allow JWT in query string for SignalR (only for /hubs/pos endpoint)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/pos"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies (BLOQUE 4)
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "ADMIN"))
    .AddPolicy("SupervisorOrAdmin", policy =>
        policy.RequireClaim("role", "ADMIN", "SUPERVISOR"))
    .AddPolicy("CashierOrAbove", policy =>
        policy.RequireClaim("role", "CASHIER", "ADMIN", "SUPERVISOR"))
    .AddPolicy("KitchenOrAbove", policy =>
        policy.RequireClaim("role", "KITCHEN", "ADMIN", "SUPERVISOR"))
    .AddPolicy("BarOrAbove", policy =>
        policy.RequireClaim("role", "BAR", "ADMIN", "SUPERVISOR"))
    .AddPolicy("WaiterOrAbove", policy =>
        policy.RequireClaim("role", "WAITER", "CASHIER", "ADMIN", "SUPERVISOR"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
// Authentication & Authorization (BLOQUE 4)
app.UseAuthentication();
app.UseAuthorization();

// License enforcement middleware (runs after authentication so claims are available)
app.UseMiddleware<BMTECHRD.Pos.Api.Middleware.LicenseMiddleware>();

app.UseApiDefaults();

// Development seeding
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<BMTECHRD.Pos.Infrastructure.Persistence.AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<BMTECHRD.Pos.Application.Abstractions.Security.IPasswordHasher>();
    var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();

    try
    {
        // ensure database created and migrations applied if any
        logger?.LogInformation("[IntegrationSeed] Environment={Env}", app.Environment.EnvironmentName);
        logger?.LogInformation("[IntegrationSeed] PasswordHasher={Hasher}", hasher?.GetType().FullName ?? "<null>");

        ctx.Database.Migrate();

        if (!ctx.Businesses.Any())
        {
            var business = new BMTECHRD.Pos.Domain.Entities.Business
            {
                Name = "BMTECHRD DEMO",
                CurrencyCode = "DOP"
            };

            var license = new BMTECHRD.Pos.Domain.Entities.License
            {
                ActivationKey = "BMT-DEMO-00000",
                Plan = BMTECHRD.Pos.Domain.Enums.LicensePlan.TRIAL_7_DAYS,
                Status = BMTECHRD.Pos.Domain.Enums.LicenseStatus.INACTIVE,
                Business = business
            };
            business.License = license;

            ctx.Businesses.Add(business);

            // admin
            var admin = new BMTECHRD.Pos.Domain.Entities.User
            {
                Username = "admin",
                PasswordHash = hasher.Hash("admin"),
                PinHash = hasher.Hash("1234"),
                Role = BMTECHRD.Pos.Domain.Enums.UserRole.ADMIN,
                Business = business
            };

            var supervisor = new BMTECHRD.Pos.Domain.Entities.User
            {
                Username = "supervisor",
                PasswordHash = hasher.Hash("supervisor"),
                PinHash = hasher.Hash("1234"),
                Role = BMTECHRD.Pos.Domain.Enums.UserRole.SUPERVISOR,
                Business = business
            };

            ctx.Users.AddRange(admin, supervisor);

            // 10 tables
            var tables = Enumerable.Range(1, 10).Select(i => new BMTECHRD.Pos.Domain.Entities.Table
            {
                Business = business,
                Number = i,
                Status = BMTECHRD.Pos.Domain.Enums.TableStatus.AVAILABLE,
                PosX = i * 10,
                PosY = i * 5
            }).ToList();

            ctx.Tables.AddRange(tables);

            ctx.SaveChanges();

            // Log seeded users (include hashes for diagnostic purposes only)
            try
            {
                var seeded = ctx.Users.Select(u => new { u.Username, u.PasswordHash, u.PinHash }).ToList();
                logger?.LogInformation("[IntegrationSeed] Seeded users: {Count}", seeded.Count);
                foreach (var u in seeded)
                {
                    logger?.LogInformation("[IntegrationSeed] User={User} PasswordHash={P} PinHash={Pin}", u.Username, u.PasswordHash, u.PinHash);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "[IntegrationSeed] Failed to enumerate seeded users");
            }
        }
        else
        {
            // If businesses already exist, still log existing admin row for diagnostics
            try
            {
                var existing = ctx.Users.Where(u => u.Username == "admin").Select(u => new { u.Username, u.PasswordHash, u.PinHash }).FirstOrDefault();
                if (existing != null)
                {
                    logger?.LogInformation("[IntegrationSeed] Existing admin found PasswordHash={P}", existing.PasswordHash);
                }
                else
                {
                    logger?.LogInformation("[IntegrationSeed] No admin user found in DB");
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "[IntegrationSeed] Failed to read existing users");
            }
        }
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "[IntegrationSeed] Exception during development seeding");
        throw;
    }
}

app.MapControllers();
app.MapHub<PosHub>("/hubs/pos");

app.Run();
public partial class Program { }
