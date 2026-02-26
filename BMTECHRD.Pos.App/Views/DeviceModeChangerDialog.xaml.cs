using System;
using System.Diagnostics;
using System.Windows;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;

namespace BMTECHRD.Pos.App.Views;

public partial class DeviceModeChangerDialog : Window
{
    private readonly DeviceModeChangerViewModel _viewModel;
    private readonly LocalDeviceConfigService _configService;
    private Services.SignalRClient? _signalR;
    private Guid _businessId;

    public DeviceModeChangerDialog()
    {
        InitializeComponent();
        _viewModel = new DeviceModeChangerViewModel();
        _configService = new LocalDeviceConfigService();
        DataContext = _viewModel;

        _viewModel.OnSaveRequested += HandleSaveAsync;
        _viewModel.OnCancelRequested += HandleCancel;
    }

    public void Initialize(DeviceMode currentMode, Services.SignalRClient signalR, Guid businessId)
    {
        _viewModel.Initialize(currentMode);
        _signalR = signalR;
        _businessId = businessId;
    }

    private async void HandleSaveAsync(DeviceMode newMode)
    {
        try
        {
            // 1) Desconectar SignalR
            if (_signalR != null)
            {
                await _signalR.LeaveBusinessAsync(_businessId);
                await _signalR.DisconnectAsync();
            }

            // 2) Guardar nueva configuración
            _configService.Save(newMode);

            // 3) Mostrar mensaje
            MessageBox.Show($"Modo cambiado a {newMode}. La aplicación se reiniciará.", 
                          "Cambio guardado", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);

            // 4) Reiniciar app
            RestartApplication();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar configuración: {ex.Message}", 
                          "Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            _viewModel.IsLoading = false;
        }
    }

    private void HandleCancel()
    {
        this.Close();
    }

    private void RestartApplication()
    {
        try
        {
            // Obtener ruta del ejecutable actual
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(exePath))
            {
                exePath = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot determine executable path");
            }

            // Iniciar nueva instancia
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            };
            Process.Start(startInfo);

            // Cerrar aplicación actual
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al reiniciar aplicación: {ex.Message}", 
                          "Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
        }
    }
}
