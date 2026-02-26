using System;
using System.Net.Http;
using System.Windows;
using BMTECHRD.Pos.App.Services;
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
        services.AddSingleton<AuthSessionService>();

        // ApiClient + AuthHeaderHandler + HttpClient
        services.AddTransient<AuthHeaderHandler>();

        // HttpClient: base URL desde config (si no está, usa localhost)
        var baseUrl = TryReadStringProperty(config, "ApiBaseUrl")
                   ?? TryReadStringProperty(config, "BaseUrl")
                   ?? TryReadStringProperty(config, "ServerUrl")
                   ?? "https://localhost:5001/";

        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        // ApiClient factory: construye HttpClient con AuthHeaderHandler y resuelve la dependencia circular
        services.AddTransient<ApiClient>(sp =>
        {
            var session = sp.GetRequiredService<AuthSessionService>();
            var authHandler = new AuthHeaderHandler(session)
            {
                InnerHandler = new System.Net.Http.HttpClientHandler()
            };

            var http = new System.Net.Http.HttpClient(authHandler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            var apiClient = new ApiClient(http);
            authHandler.SetApiClient(apiClient);
            return apiClient;
        });

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

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