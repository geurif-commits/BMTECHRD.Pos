using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.Views.Admin;

public partial class AdminView : UserControl, INotifyPropertyChanged
{
    private Services.ApiClient? _api;
    private Services.SignalRClient? _signalR;
    private System.Guid _businessId;
    private string? _userRole;
    private bool _canConfigureDevice;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool CanConfigureDevice
    {
        get => _canConfigureDevice;
        set
        {
            if (_canConfigureDevice == value) return;
            _canConfigureDevice = value;
            OnPropertyChanged();
        }
    }

    public AdminView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Initialize(ApiClient api, System.Guid businessId, System.Guid actorUserId, Services.SignalRClient? signalR = null, string? userRole = null)
    {
        _api = api;
        _signalR = signalR;
        _businessId = businessId;
        _userRole = userRole;

        // Mostrar botón "Configurar equipo" solo para ADMIN o SUPERVISOR (ETAPA 8.1)
        CanConfigureDevice = !string.IsNullOrEmpty(userRole) && 
                            (userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) || 
                             userRole.Equals("SUPERVISOR", StringComparison.OrdinalIgnoreCase));

        UsersView.Initialize(api, businessId, actorUserId);
        InventoryView.Initialize(api, businessId);
        ShiftsView.Initialize(api, businessId, actorUserId);
        ReportsView.Initialize(api, businessId, actorUserId);
    }

    private void OnConfigureDeviceClick(object sender, RoutedEventArgs e)
    {
        if (_signalR == null)
        {
            MessageBox.Show("No hay conexión disponible para cambiar modo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var configService = new LocalDeviceConfigService();
        var currentConfig = configService.Load();
        var currentMode = currentConfig?.Mode ?? DeviceMode.Server;

        var dialog = new DeviceModeChangerDialog();
        dialog.Initialize(currentMode, _signalR, _businessId);
        dialog.Owner = Window.GetWindow(this);
        dialog.ShowDialog();
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
