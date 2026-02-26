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
    }

    private void SelectMode(DeviceMode mode)
    {
        _configService.Save(mode);
        System.Windows.Application.Current.Shutdown(0);
    }

    private void OnServerClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Server);
    private void OnCashierClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Cashier);
    private void OnKitchenClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Kitchen);
    private void OnBarClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Bar);
    private void OnFloorClick(object sender, RoutedEventArgs e) => SelectMode(DeviceMode.Floor);
}
