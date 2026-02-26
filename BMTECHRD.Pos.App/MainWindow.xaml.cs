using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Core;
using Microsoft.Extensions.DependencyInjection;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App
{
    public partial class MainWindow : Window
    {
        private DeviceMode _deviceMode;
        private readonly LocalDeviceConfigService _configService;
        private AuthSessionService? _authSession;
        private Services.SignalRClient? _signalR;

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            // Ensure UI updates happen on UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
            {
                try
                {
                    if (_signalR != null)
                    {
                        await _signalR.DisconnectAsync();
                        _signalR = null;
                    }
                }
                catch
                {
                    // ignore
                }

                // Navigate back to StartView
                var api = App.Services.GetService<Services.ApiClient>();
                var start = new Views.StartView();
                start.Initialize(api!, _authSession!);
                Content = start;
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            _configService = new LocalDeviceConfigService();
        }

        public void SetContent(object content)
        {
            // Simple wrapper to set the named ContentControl in XAML
            if (content is UIElement el)
            {
                MainContent.Content = el;
            }
            else
            {
                MainContent.Content = content;
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var config = _configService.Load();
            if (config == null)
            {
                Close();
                return;
            }

            _deviceMode = config.Mode;

            // ✅ ETAPA 9: sesión con DeviceId persistente (obtenida desde DI)
            _authSession = (AuthSessionService?)App.Services.GetService(typeof(AuthSessionService));
            if (_authSession == null)
            {
                MessageBox.Show("Error inicializando la sesión de autenticación. Reinicia la aplicación.", "Error DI", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Base URL: si tu config tiene ApiBaseUrl úsalo, si no localhost
            var baseUrl = GetBaseUrlFromConfigOrDefault(config);

            // Obtener ApiClient (typed client) desde DI - HttpClientFactory maneja handlers
            var api = (Services.ApiClient?)App.Services.GetService(typeof(Services.ApiClient));
            if (api == null)
            {
                MessageBox.Show("Error inicializando el cliente API. Revisa la configuración y reinicia.", "Error DI", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Start view (business selection + login)
            var start = new Views.StartView();
            start.Initialize(api, _authSession);

            start.OnLoginSuccess += session =>
            {
                // Guardar sesión auth
                _authSession!.SetSession(
                    session.AccessToken,
                    session.RefreshToken,
                    session.UserId,
                    session.BusinessId,
                    session.Username,
                    session.Role,
                    session.ExpiresAt);

                // Subscribe to session expired to cleanup SignalR and navigate back to StartView
                _authSession.SessionExpired -= OnSessionExpired;
                _authSession.SessionExpired += OnSessionExpired;

                // SignalR client (usa access token actual)
                _signalR = new Services.SignalRClient(baseUrl, () => Task.FromResult(_authSession!.AccessToken));

                // Connect and join business asynchronously so we don't block UI thread
                Task.Run(async () =>
                {
                    try
                    {
                        await _signalR.ConnectAsync();
                        await _signalR.JoinBusinessAsync(session.BusinessId);
                    }
                    catch
                    {
                        // ignore connect errors for now
                    }

                    // Update UI on dispatcher
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Validar rol por device mode
                        if (!ValidateRoleForDeviceMode(session.Role))
                        {
                            var accessDenied = new Views.AccessDeniedView();
                            accessDenied.Initialize(_signalR, session.BusinessId);
                            Content = accessDenied;
                            return;
                        }

                        // Cargar vista según device mode
                        LoadViewForDeviceMode(api, _signalR, session);
                    });
                });
            };

            Content = start;
        }

        private static string GetBaseUrlFromConfigOrDefault(DeviceConfig config)
        {
            // Si tu LocalDeviceConfig tiene otra propiedad, cámbiala aquí (ApiBaseUrl/BaseUrl/ServerUrl).
            // Si NO existe, cae a localhost.
            var url = TryReadStringProperty(config, "ApiBaseUrl")
                   ?? TryReadStringProperty(config, "BaseUrl")
                   ?? TryReadStringProperty(config, "ServerUrl")
                   ?? "https://localhost:5001/";

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

        private bool ValidateRoleForDeviceMode(string? role)
        {
            var userRole = role?.ToUpperInvariant() ?? "";

            return _deviceMode switch
            {
                DeviceMode.Server => userRole == "ADMIN" || userRole == "SUPERVISOR",
                DeviceMode.Cashier => userRole == "CASHIER" || userRole == "ADMIN" || userRole == "SUPERVISOR",
                DeviceMode.Kitchen => userRole == "KITCHEN" || userRole == "ADMIN" || userRole == "SUPERVISOR",
                DeviceMode.Bar => userRole == "BAR" || userRole == "ADMIN" || userRole == "SUPERVISOR",
                DeviceMode.Floor => userRole == "WAITER" || userRole == "CASHIER" || userRole == "ADMIN" || userRole == "SUPERVISOR",
                _ => false
            };
        }

        private void LoadViewForDeviceMode(Services.ApiClient api, Services.SignalRClient signalR, SessionModel session)
        {
            switch (_deviceMode)
            {
                case DeviceMode.Server:
                    LoadServerView(api, signalR, session);
                    break;

                case DeviceMode.Cashier:
                    LoadCashierView(api, signalR, session);
                    break;

                case DeviceMode.Kitchen:
                    LoadKitchenView(api, signalR, session.BusinessId);
                    break;

                case DeviceMode.Bar:
                    LoadBarView(api, signalR, session.BusinessId);
                    break;

                case DeviceMode.Floor:
                    LoadFloorView(api, signalR, session);
                    break;
            }
        }

        private void LoadServerView(Services.ApiClient api, Services.SignalRClient signalR, SessionModel session)
        {
            var adminView = new Views.Admin.AdminView();
            adminView.Initialize(api, session.BusinessId, session.UserId, signalR, session.Role);
            Content = adminView;
        }

        private void LoadCashierView(Services.ApiClient api, Services.SignalRClient signalR, SessionModel session)
        {
            var tab = new TabControl();

            var cashView = new Views.CashierView();
            cashView.Initialize(api, signalR, session.BusinessId, session.UserId);
            tab.Items.Add(new TabItem { Header = "Caja", Content = cashView });

            if (session.Role?.ToUpperInvariant() == "SUPERVISOR")
            {
                var adminView = new Views.Admin.AdminView();
                adminView.Initialize(api, session.BusinessId, session.UserId);
                tab.Items.Add(new TabItem { Header = "Admin", Content = adminView });
            }

            tab.SelectedIndex = 0;
            Content = tab;
        }

        private void LoadKitchenView(Services.ApiClient api, Services.SignalRClient signalR, Guid businessId)
        {
            var kitchenView = new Views.KitchenQueueView();
            kitchenView.Initialize(api, signalR, businessId);
            Content = kitchenView;
        }

        private void LoadBarView(Services.ApiClient api, Services.SignalRClient signalR, Guid businessId)
        {
            var barView = new Views.BarQueueView();
            barView.Initialize(api, signalR, businessId);
            Content = barView;
        }

        private void LoadFloorView(Services.ApiClient api, Services.SignalRClient signalR, SessionModel session)
        {
            var tablesView = new Views.TablesMapView();
            tablesView.Initialize(api, signalR, session.BusinessId, session.UserId);
            Content = tablesView;
        }
    }
}