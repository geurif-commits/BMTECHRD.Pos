using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;

namespace BMTECHRD.Pos.App.Views;

public partial class StartView : UserControl
{
    private StartViewModel? _vm;
    public event Action<SessionModel>? OnLoginSuccess;

    public StartView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api)
    {
        _vm = new StartViewModel(api);
        DataContext = _vm;
        _vm.OnLoginSuccess += s => OnLoginSuccess?.Invoke(s);
        _vm.PropertyChanged += VmOnPropertyChanged;

        BtnLogin.Click += (_, _) =>
        {
            if (_vm == null) return;
            _vm.Username = TxtUsername.Text;
            _vm.Password = TxtPassword.Password;
            _vm.LoginCommand.Execute(null);
        };

        BtnClearQuickLogin.Click += (_, _) => _vm?.ClearQuickLoginCommand.Execute(null);

        CbBusinesses.SelectionChanged += (_, _) =>
        {
            if (CbBusinesses.SelectedItem is BusinessPublicModel b)
            {
                if (!string.IsNullOrEmpty(b.LogoPath))
                {
                    try
                    {
                        var baseUri = api.BaseAddress ?? new Uri(LocalDeviceConfigService.DefaultApiBaseUrl);
                        ImgLogo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(baseUri, b.LogoPath));
                        ImgLogo.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        ImgLogo.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ImgLogo.Visibility = Visibility.Collapsed;
                }
            }
        };

        _vm.LoadBusinessesCommand.Execute(null);
        RefreshQuickLoginUi();
        TxtError.Text = _vm.Error ?? string.Empty;
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_vm == null) return;

        if (e.PropertyName == nameof(StartViewModel.Error) || e.PropertyName == null)
        {
            TxtError.Text = _vm.Error ?? string.Empty;
        }

        if (e.PropertyName == nameof(StartViewModel.HasQuickLogin)
            || e.PropertyName == nameof(StartViewModel.QuickLoginLabel)
            || e.PropertyName == null)
        {
            RefreshQuickLoginUi();
        }
    }

    private void RefreshQuickLoginUi()
    {
        if (_vm == null) return;

        if (_vm.HasQuickLogin)
        {
            TxtQuickHint.Text = _vm.QuickLoginLabel;
            TxtQuickHint.Visibility = Visibility.Visible;
            BtnClearQuickLogin.Visibility = Visibility.Visible;
            return;
        }

        TxtQuickHint.Visibility = Visibility.Collapsed;
        BtnClearQuickLogin.Visibility = Visibility.Collapsed;
    }
}
