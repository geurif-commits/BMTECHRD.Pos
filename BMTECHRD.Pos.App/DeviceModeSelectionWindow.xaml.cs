using System;
using System.Windows;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App;

public partial class DeviceModeSelectionWindow : Window
{
    private readonly LocalDeviceConfigService _configService;

    public DeviceModeSelectionWindow()
    {
        InitializeComponent();
        _configService = new LocalDeviceConfigService();

        var current = _configService.Load();
        TxtApiBaseUrl.Text = current?.ApiBaseUrl ?? LocalDeviceConfigService.DefaultApiBaseUrl;
    }

    private void SelectMode(DeviceMode mode)
    {
        var rawUrl = TxtApiBaseUrl.Text?.Trim();
        if (string.IsNullOrWhiteSpace(rawUrl))
            rawUrl = LocalDeviceConfigService.DefaultApiBaseUrl;

        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out _))
        {
            MessageBox.Show("La URL API no es válida. Ejemplo: http://localhost:5139/", "URL inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _configService.Save(mode, rawUrl);

        DialogResult = true;
        Close();
    }

    private void OnServerClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Server);
    private void OnCashierClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Cashier);
    private void OnKitchenClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Kitchen);
    private void OnBarClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Bar);
    private void OnFloorClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Floor);
}
