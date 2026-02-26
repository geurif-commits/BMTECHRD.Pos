using System;
using System.Net.Http;
using System.Windows;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace BMTECHRD.Pos.App;

public partial class App : Application
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

    private static IServiceProvider ConfigureServices(LocalDeviceConfig config)
    {
        var services = new ServiceCollection();

        // Session singleton (tokens + DeviceId persistente)
        services.AddSingleton<AuthSessionService>();

        // ApiClient + AuthHeaderHandler + HttpClient
        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<ApiClient>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();

        // HttpClient: base URL desde config (si no está, usa localhost)
        var baseUrl = NormalizeBaseUrl(config.ApiBaseUrl) ?? "https://localhost:5001/";

        services.AddHttpClient<ApiClient>(http =>
        {
            http.BaseAddress = new Uri(baseUrl);
            http.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

        return services.BuildServiceProvider();
    }

    private static string? NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        url = url.Trim();
        if (!url.EndsWith("/")) url += "/";

        return url;
    }
}