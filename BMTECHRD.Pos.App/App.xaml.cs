using System;
using System.Net.Http;
using System.Windows;
using BMTECHRD.Pos.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BMTECHRD.Pos.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configService = new LocalDeviceConfigService();
        var config = configService.Load();

        // Primer arranque: seleccionar modo+URL y continuar en el mismo ciclo de inicio.
        if (config == null)
        {
            var selectionWindow = new DeviceModeSelectionWindow();
            selectionWindow.ShowDialog();
            config = configService.Load();
            if (config == null)
            {
                Shutdown();
                return;
            }
        }

        Services = ConfigureServices(config);

        var main = new MainWindow();
        MainWindow = main;
        main.Show();
    }

    private static IServiceProvider ConfigureServices(LocalDeviceConfig config)
    {
        var services = new ServiceCollection();

        services.AddSingleton<AuthSessionService>();
        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<ApiClient>();

        var baseUrl = NormalizeBaseUrl(config.ApiBaseUrl) ?? LocalDeviceConfigService.DefaultApiBaseUrl;

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
