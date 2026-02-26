using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;
using Serilog;
using BMTECHRD.Pos.App.Core;

Console.WriteLine("Starting harness simulation...");

// configure Serilog for harness
try
{
    var dir = AppPaths.LogsFolder;
    System.IO.Directory.CreateDirectory(dir);
    var path = System.IO.Path.Combine(dir, "auth.log");
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.File(path, rollingInterval: Serilog.RollingInterval.Day)
        .CreateLogger();
}
catch
{
}

var services = new ServiceCollection();

// Register session
services.AddSingleton<AuthSessionService>();

// Register AuthHeaderHandler
services.AddTransient<AuthHeaderHandler>();

// Shared simulation state
var simState = new SimState();

// Register AuthClient with fake handler
services.AddHttpClient<AuthClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5001/");
    c.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(simState));

// Register ApiClient with AuthHeaderHandler in pipeline and fake primary handler
services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5001/");
    c.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthHeaderHandler>()
.ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(simState));

var provider = services.BuildServiceProvider();

var session = provider.GetRequiredService<AuthSessionService>();

// Seed session with expired access token and valid refresh token
session.SetSession(accessToken: "expired-access", refreshToken: "valid-refresh", userId: Guid.NewGuid(), businessId: Guid.NewGuid(), username: "test", role: "ADMIN", expiresAt: DateTime.UtcNow.AddMinutes(-10));

Console.WriteLine($"Initial DeviceId: {session.DeviceId}");

var api = provider.GetRequiredService<ApiClient>();

Console.WriteLine("Calling protected endpoint (should trigger 401 -> refresh -> retry)...");

try
{
    var tables = await api.GetTablesAsync(Guid.NewGuid());
    Console.WriteLine($"Result: tables count = {tables.Count}");
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex}");
}

Console.WriteLine("Simulation finished.");

// --- simulation helpers ---

record SimState
{
    public bool Refreshed { get; set; }
}

class FakeAuthHandler : HttpMessageHandler
{
    private readonly SimState _state;
    public FakeAuthHandler(SimState state) => _state = state;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        // Simulate refresh endpoint
        if (request.RequestUri?.AbsolutePath?.Contains("/api/auth/refresh") == true)
        {
            _state.Refreshed = true;
            var resp = new LoginResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                UserId = Guid.NewGuid(),
                BusinessId = Guid.NewGuid(),
                Username = "test",
                Role = "ADMIN",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            var json = JsonSerializer.Serialize(resp);
            var http = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(http);
        }

        // default
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

class FakeApiHandler : HttpMessageHandler
{
    private readonly SimState _state;
    public FakeApiHandler(SimState state) => _state = state;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        // Protected endpoint
        if (request.RequestUri?.AbsolutePath?.Contains("/api/tables") == true)
        {
            if (!_state.Refreshed)
            {
                // return 401 to trigger refresh
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            else
            {
                // return empty list
                var json = JsonSerializer.Serialize(Array.Empty<object>());
                var http = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(http);
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
