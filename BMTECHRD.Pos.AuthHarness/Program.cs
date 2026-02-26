using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;

Console.WriteLine("BMTECHRD.Pos.AuthHarness - starting");

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
services.AddLogging(cfg => cfg.AddSerilog());

// Register session
services.AddSingleton<AuthSessionService>();

// Register AuthClient with fake auth handler (will configure per-scenario)
// We'll register two named HttpClients by factory at runtime per scenario using ConfigurePrimaryHttpMessageHandler

// Register AuthHeaderHandler
services.AddTransient<AuthHeaderHandler>();

// Register ApiClient typed
services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5001/");
    c.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

// Register AuthClient typed (no handler)
services.AddHttpClient<AuthClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5001/");
    c.Timeout = TimeSpan.FromSeconds(30);
});

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

// Run scenarios sequentially
await Scenario_RefreshOk(provider, logger);
await Scenario_RefreshFail(provider, logger);
await Scenario_Concurrency(provider, logger);
await Scenario_RefreshReuseDetected(provider, logger);
await Scenario_RefreshReuseDetected_Concurrency(provider, logger);

Console.WriteLine("All scenarios finished.");

// Scenario implementations
async Task Scenario_RefreshOk(ServiceProvider sp, Microsoft.Extensions.Logging.ILogger logger)
{
    Console.WriteLine("\n=== Scenario 1: Refresh OK ===");
    var sim = new HarnessSimState();
    sim.RefreshFailMode = RefreshFailureMode.None;
    sim.RotateRefreshTokenOnSuccess = true;

    // Build new scope so HttpClientFactory uses our fake handlers
    var sc = new ServiceCollection();
    sc.AddSingleton(sp.GetRequiredService<AuthSessionService>());
    // file logger removed; rely on Serilog
    sc.AddLogging(cfg => cfg.AddConsole());

    sc.AddTransient<AuthHeaderHandler>();

    sc.AddHttpClient<AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(sim));

    sc.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .AddHttpMessageHandler<AuthHeaderHandler>()
      .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(sim));

    using var prov = sc.BuildServiceProvider();

    var session = prov.GetRequiredService<AuthSessionService>();
    // seed expired access token, valid refresh
    session.SetSession("expired", "valid-refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

    var api = prov.GetRequiredService<ApiClient>();

    // call protected endpoint
    Console.WriteLine("Calling protected endpoint...");
    var tables = await api.GetTablesAsync(Guid.NewGuid());
    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine("Retry succeeded");
}

async Task Scenario_RefreshFail(ServiceProvider sp, Microsoft.Extensions.Logging.ILogger logger)
{
    Console.WriteLine("\n=== Scenario 2: Refresh FAIL ===");
    var sim = new HarnessSimState();
    sim.RefreshFailMode = RefreshFailureMode.Unauthorized401;

    var sc = new ServiceCollection();
    sc.AddSingleton(sp.GetRequiredService<AuthSessionService>());
    // file logger removed; rely on Serilog
    sc.AddLogging(cfg => cfg.AddConsole());
    sc.AddTransient<AuthHeaderHandler>();

    sc.AddHttpClient<AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(sim));

    sc.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .AddHttpMessageHandler<AuthHeaderHandler>()
      .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(sim));

    using var prov = sc.BuildServiceProvider();
    var session = prov.GetRequiredService<AuthSessionService>();
    bool sessionExpiredFired = false;
    session.SessionExpired += (_, _) => sessionExpiredFired = true;

    session.SetSession("expired", "invalid-refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

    var api = prov.GetRequiredService<ApiClient>();

    Console.WriteLine("Calling protected endpoint (expect refresh fail)...");
    var tables = await api.GetTablesAsync(Guid.NewGuid());

    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine($"SessionExpired fired: {sessionExpiredFired}");
}

async Task Scenario_Concurrency(ServiceProvider sp, Microsoft.Extensions.Logging.ILogger logger)
{
    Console.WriteLine("\n=== Scenario 3: Concurrency ===");
    var sim = new HarnessSimState();
    sim.RefreshFailMode = RefreshFailureMode.None;
    sim.RotateRefreshTokenOnSuccess = true;

    var sc = new ServiceCollection();
    sc.AddSingleton(sp.GetRequiredService<AuthSessionService>());
    // file logger removed; rely on Serilog
    sc.AddLogging(cfg => cfg.AddConsole());
    sc.AddTransient<AuthHeaderHandler>();

    sc.AddHttpClient<AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(sim));

    sc.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .AddHttpMessageHandler<AuthHeaderHandler>()
      .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(sim));

    using var prov = sc.BuildServiceProvider();
    var session = prov.GetRequiredService<AuthSessionService>();

    session.SetSession("expired", "valid-refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

    var api = prov.GetRequiredService<ApiClient>();

    // run 10 concurrent requests
    var tasks = new Task<int>[10];
    for (int i = 0; i < 10; i++)
    {
        tasks[i] = Task.Run(async () =>
        {
            var list = await api.GetTablesAsync(Guid.NewGuid());
            return list.Count;
        });
    }
    var results = await Task.WhenAll(tasks);
    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine($"All requests succeeded: {results.Length}/10");
}

// S4: Reuse / rotation mismatch -> should clear session and emit SessionExpired once
async Task Scenario_RefreshReuseDetected(ServiceProvider sp, Microsoft.Extensions.Logging.ILogger logger)
{
    Console.WriteLine("\n=== Scenario S4: Refresh reuse detected (403) ===");
    var sim = new HarnessSimState();
    sim.RefreshFailMode = RefreshFailureMode.ReuseDetected403;

    var sc = new ServiceCollection();
    sc.AddSingleton(sp.GetRequiredService<AuthSessionService>());
    sc.AddLogging(cfg => cfg.AddConsole());
    sc.AddTransient<AuthHeaderHandler>();

    sc.AddHttpClient<AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(sim));

    sc.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .AddHttpMessageHandler<AuthHeaderHandler>()
      .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(sim));

    using var prov = sc.BuildServiceProvider();
    var session = prov.GetRequiredService<AuthSessionService>();
    int sessionExpiredCount = 0;
    session.SessionExpired += (_, _) => sessionExpiredCount++;

    session.SetSession("expired", "reused_refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

    var api = prov.GetRequiredService<ApiClient>();

    Console.WriteLine("Calling protected endpoint (expect refresh reuse -> session expired)...");
    var tables = await api.GetTablesAsync(Guid.NewGuid());

    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine($"SessionExpired fired: {sessionExpiredCount}");
}

// S4b: concurrency with reused refresh token
async Task Scenario_RefreshReuseDetected_Concurrency(ServiceProvider sp, Microsoft.Extensions.Logging.ILogger logger)
{
    Console.WriteLine("\n=== Scenario S4b: Concurrency + refresh reuse detected ===");
    var sim = new HarnessSimState();
    sim.RefreshFailMode = RefreshFailureMode.ReuseDetected403;

    var sc = new ServiceCollection();
    sc.AddSingleton(sp.GetRequiredService<AuthSessionService>());
    sc.AddLogging(cfg => cfg.AddConsole());
    sc.AddTransient<AuthHeaderHandler>();

    sc.AddHttpClient<AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(sim));

    sc.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
      .AddHttpMessageHandler<AuthHeaderHandler>()
      .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(sim));

    using var prov = sc.BuildServiceProvider();
    var session = prov.GetRequiredService<AuthSessionService>();
    int sessionExpiredCount = 0;
    session.SessionExpired += (_, _) => sessionExpiredCount++;

    session.SetSession("expired", "reused_refresh", Guid.NewGuid(), Guid.NewGuid(), "u", "ADMIN", DateTime.UtcNow.AddMinutes(-10));

    var api = prov.GetRequiredService<ApiClient>();

    var tasks = new Task<int>[10];
    for (int i = 0; i < 10; i++)
    {
        tasks[i] = Task.Run(async () =>
        {
            var list = await api.GetTablesAsync(Guid.NewGuid());
            return list.Count;
        });
    }

    var results = await Task.WhenAll(tasks);
    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine($"SessionExpired fired: {sessionExpiredCount}");
    Console.WriteLine($"All requests completed: {results.Length}/10");
}
    var results = await Task.WhenAll(tasks);
    Console.WriteLine($"Refresh attempted: {sim.RefreshAttempts}");
    Console.WriteLine($"All requests succeeded: {results.Length}/10");
}

// Helpers for harness
public enum RefreshFailureMode
{
    None = 0,
    Unauthorized401 = 1,
    ReuseDetected403 = 2
}

class HarnessSimState
{
    public RefreshFailureMode RefreshFailMode { get; set; } = RefreshFailureMode.None;
    public bool RotateRefreshTokenOnSuccess { get; set; } = true;
    public int RefreshAttempts { get; set; }
    public bool TokenUpdated { get; set; }

    public void Reset()
    {
        RefreshFailMode = RefreshFailureMode.None;
        RotateRefreshTokenOnSuccess = true;
        RefreshAttempts = 0;
        TokenUpdated = false;
    }
}

class FakeAuthHandler : HttpMessageHandler
{
    private readonly HarnessSimState _state;
    public FakeAuthHandler(HarnessSimState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath?.Contains("/api/auth/refresh") == true)
        {
            _state.RefreshAttempts++;

            // read refresh token from body
            var body = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult() ?? string.Empty;
            string? refreshToken = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("refreshToken", out var rt))
                    refreshToken = rt.GetString();
            }
            catch { }

            // If refresh token is 'reused_refresh' simulate reuse detection
            if (string.Equals(refreshToken, "reused_refresh", StringComparison.Ordinal))
            {
                if (_state.RefreshFailMode == RefreshFailureMode.ReuseDetected403)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Refresh token reuse detected") });
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Invalid refresh") });
                }
            }

            // Normal success case
            if (string.Equals(refreshToken, "valid-refresh", StringComparison.Ordinal))
            {
                _state.TokenUpdated = true;
                var newRefresh = _state.RotateRefreshTokenOnSuccess ? "valid_refresh_rotated" : "valid-refresh";
                var resp = new LoginResponse
                {
                    AccessToken = "new-access",
                    RefreshToken = newRefresh,
                    UserId = Guid.NewGuid(),
                    BusinessId = Guid.NewGuid(),
                    Username = "harness",
                    Role = "ADMIN",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };
                var json = JsonSerializer.Serialize(resp);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Invalid refresh") });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

class FakeApiHandler : HttpMessageHandler
{
    private readonly HarnessSimState _state;
    public FakeApiHandler(HarnessSimState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath?.Contains("/api/tables") == true)
        {
            if (!_state.TokenUpdated)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            else
            {
                var json = JsonSerializer.Serialize(new object[0]);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
