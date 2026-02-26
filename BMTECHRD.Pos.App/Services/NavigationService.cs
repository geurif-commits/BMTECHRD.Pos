using BMTECHRD.Pos.App.ViewModels.Auth;
using BMTECHRD.Pos.App.Views.Auth;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BMTECHRD.Pos.App.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _sp;

    public NavigationService(IServiceProvider sp)
    {
        _sp = sp;
    }

    public Task GoToLoginAsync()
    {
        var vm = _sp.GetRequiredService<LoginViewModel>();
        var view = new LoginView { DataContext = vm };
        SetMainContent(view);
        return Task.CompletedTask;
    }

    public Task GoToShellAsync()
    {
        // Placeholder hasta que conectemos tu Shell real
        var content = new TextBlock
        {
            Text = "✅ Login OK. Aquí va tu Shell/Dashboard según Device Mode y Rol.",
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        SetMainContent(content);
        return Task.CompletedTask;
    }

    private static void SetMainContent(object content)
    {
        if (Application.Current.MainWindow is MainWindow mw)
        {
            mw.SetContent(content);
        }
        else if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Content = content;
        }
    }
}