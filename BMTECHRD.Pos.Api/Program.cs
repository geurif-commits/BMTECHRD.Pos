using BMTECHRD.Pos.Api.Hubs;
using BMTECHRD.Pos.Api.Extensions;
using BMTECHRD.Pos.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

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

// Authentication first so middleware can read claims (businessId from token)
app.UseAuthentication();

// License enforcement middleware
app.UseMiddleware<BMTECHRD.Pos.Api.Middleware.LicenseMiddleware>();

// Authorization
app.UseAuthorization();

app.UseApiDefaults();

// Development seeding
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<BMTECHRD.Pos.Infrastructure.Persistence.AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<BMTECHRD.Pos.Application.Abstractions.Security.IPasswordHasher>();

    // ensure database created and migrations applied if any
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
    }
    else
    {
        // Ensure each business has a license record (legacy DB safety)
        var businessesWithoutLicense = ctx.Businesses
            .Where(b => !ctx.Licenses.Any(l => l.BusinessId == b.Id))
            .ToList();

        if (businessesWithoutLicense.Count > 0)
        {
            foreach (var b in businessesWithoutLicense)
            {
                var fallbackKey = b.Name == "BMTECHRD DEMO"
                    ? "BMT-DEMO-00000"
                    : $"BMT-{b.Id:N}"[..14].ToUpperInvariant();

                ctx.Licenses.Add(new BMTECHRD.Pos.Domain.Entities.License
                {
                    Id = Guid.NewGuid(),
                    BusinessId = b.Id,
                    Business = b,
                    Plan = BMTECHRD.Pos.Domain.Enums.LicensePlan.TRIAL_7_DAYS,
                    Status = BMTECHRD.Pos.Domain.Enums.LicenseStatus.INACTIVE,
                    ActivationKey = fallbackKey,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            ctx.SaveChanges();
        }
    }
}

app.MapControllers();
app.MapHub<PosHub>("/hubs/pos");

app.Run();