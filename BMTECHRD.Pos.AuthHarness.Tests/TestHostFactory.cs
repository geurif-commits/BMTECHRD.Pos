using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using BMTECHRD.Pos.AuthHarness.Shared;
using BMTECHRD.Pos.Auth.Core.Services;
using BMTECHRD.Pos.Auth.Core.Interfaces;
using BMTECHRD.Pos.App.Services;

public static class TestHostFactory
{
    public static ServiceProvider Build(HarnessSimState state)
    {
        // minimal Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory>(_ => new SerilogLoggerFactory(Log.Logger, dispose: false));
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // state and session (core session)
        services.AddSingleton(state);
        services.AddSingleton<AuthSessionService>();

        // handlers (core)
        services.AddTransient<AuthHeaderHandler>();

        // AuthClient (core) configured to use FakeAuthHandler
        services.AddHttpClient<IAuthClient, BMTECHRD.Pos.Auth.Core.Services.AuthClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
            .ConfigurePrimaryHttpMessageHandler(() => new FakeAuthHandler(state));

        // IApiClient -> TestApiClient using FakeApiHandler as primary handler; include AuthHeaderHandler in pipeline
        services.AddHttpClient<IApiClient, TestApiClient>(c => { c.BaseAddress = new Uri("https://localhost:5001/"); })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new FakeApiHandler(state));

        return services.BuildServiceProvider();
    }
}
