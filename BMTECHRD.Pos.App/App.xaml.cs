using System;
using System.Net.Http;
using System.Windows;
using BMTECHRD.Pos.App.Services;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using BMTECHRD.Pos.App.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace BMTECHRD.Pos.App;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1) Device mode config (ETAPA 8/8.1)
        var configService = new LocalDeviceConfigService();
        var config = configService.Load();

        if (config == null)
        {
            var selectionWindow = new DeviceModeSelectionWindow();
            selectionWindow.ShowDialog();
            return;
        }

        // 2) DI container (ETAPA 9 WPF Auth)
        // configure Serilog file sink for auth traces
        try
        {
            var logDir = BMTECHRD.Pos.App.Core.AppPaths.LogsFolder;
            System.IO.Directory.CreateDirectory(logDir);
            var path = System.IO.Path.Combine(logDir, "auth-.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: path,
                    rollingInterval: Serilog.RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console()
                .CreateLogger();
        }
        catch
        {
            // ignore logging setup failures
        }

        Services = ConfigureServices(config);

        // 3) Iniciar MainWindow (controla navegación)
        var main = new MainWindow();
        MainWindow = main;
        main.Show();

        // 4) Navegar a login inicialmente
        var nav = Services.GetRequiredService<INavigationService>();
        _ = nav.GoToLoginAsync();
    }

    private static ServiceProvider ConfigureServices(DeviceConfig config)
    {
        var services = new ServiceCollection();

        // Session singleton (tokens + DeviceId persistente)
        services.AddSingleton<BMTECHRD.Pos.Auth.Core.Services.AuthSessionService>();

        // HttpClient: base URL desde config (si no está, usa localhost)
        var baseUrl = TryReadStringProperty(config, "ApiBaseUrl")
                   ?? TryReadStringProperty(config, "BaseUrl")
                   ?? TryReadStringProperty(config, "ServerUrl")
                   ?? "https://localhost:5001/";

        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        // Logging (Serilog)
        services.AddLogging(builder => builder.AddSerilog());

        // Auth client (no handlers) - used for login/refresh to avoid recursion
        services.AddHttpClient<BMTECHRD.Pos.Auth.Core.Interfaces.IAuthClient, BMTECHRD.Pos.Auth.Core.Services.AuthClient>(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register AuthHeaderHandler so it can be used as a message handler
        services.AddTransient<BMTECHRD.Pos.Auth.Core.Services.AuthHeaderHandler>();

        // ApiClient typed client that uses AuthHeaderHandler to attach tokens and refresh
        services.AddHttpClient<ApiClient>(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddHttpMessageHandler<BMTECHRD.Pos.Auth.Core.Services.AuthHeaderHandler>();

        // Expose IApiClient (core contract) using adapter to App ApiClient implementation
        services.AddSingleton<BMTECHRD.Pos.Auth.Core.Interfaces.IApiClient>(sp => new BMTECHRD.Pos.App.Services.ApiClientAdapter(sp.GetRequiredService<ApiClient>()));

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDeviceRolePolicy, DeviceRolePolicy>();

        // ViewModels
        services.AddTransient<LoginViewModel>();

        // Note: ApiClient registered via factory above. No AddHttpClient used to avoid extra package dependency.

        return services.BuildServiceProvider();
    }

    private static string? NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        url = url.Trim();
        if (!url.EndsWith('/')) url += "/";

        return url;
    }

    private static string? TryReadStringProperty(object obj, string propName)
    {
        try
        {
            var p = obj.GetType().GetProperty(propName);
            if (p == null) return null;
            return p.GetValue(obj) as string;
        }
        catch
        {
            return null;
        }
    }
}