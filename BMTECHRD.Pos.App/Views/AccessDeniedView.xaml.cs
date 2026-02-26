using System.Windows;
using System.Windows.Controls;

namespace BMTECHRD.Pos.App.Views;

public partial class AccessDeniedView : UserControl
{
    private Services.SignalRClient? _signalR;
    private System.Guid _businessId;

    public AccessDeniedView()
    {
        InitializeComponent();
    }

    public void Initialize(Services.SignalRClient signalR, System.Guid businessId)
    {
        _signalR = signalR;
        _businessId = businessId;
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        _ = HandleLogoutAsync();
    }

    private async System.Threading.Tasks.Task HandleLogoutAsync()
    {
        if (_signalR != null)
        {
            try
            {
                await _signalR.LeaveBusinessAsync(_businessId);
                await _signalR.DisconnectAsync();
            }
            catch
            {
                // Ignorar errores al desconectar
            }
        }

        // Cerrar la aplicación y permitir reinicio manual
        System.Windows.Application.Current.Shutdown();
    }
}
