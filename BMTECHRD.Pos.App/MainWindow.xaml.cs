using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App
{
    public partial class MainWindow : Window
    {
        private DeviceMode _deviceMode;
        private readonly LocalDeviceConfigService _configService;
        private AuthSessionService? _authSession;

        public MainWindow()
        {
            InitializeComponent();
            _configService = new LocalDeviceConfigService();
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

            // ✅ ETAPA 9: sesión con DeviceId persistente
            _authSession = new AuthSessionService();

            // Base URL: si tu config tiene ApiBaseUrl úsalo, si no localhost
            var baseUrl = GetBaseUrlFromConfigOrDefault(config);

            // ✅ Un solo handler + un solo HttpClient
            var authHandler = new AuthHeaderHandler(_authSession)
            {
                InnerHandler = new HttpClientHandler()
            };

            var http = new HttpClient(authHandler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            var api = new Services.ApiClient(http);

            // ✅ Resolver ciclo: ahora el handler ya conoce el ApiClient real
            authHandler.SetApiClient(api);

            // Start view (business selection + login)
            var start = new Views.StartView();
            start.Initialize(api);

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

                // SignalR client (usa access token actual)
                var signalR = new Services.SignalRClient(baseUrl, () => Task.FromResult(_authSession!.AccessToken));
                signalR.ConnectAsync().GetAwaiter().GetResult();
                signalR.JoinBusinessAsync(session.BusinessId).GetAwaiter().GetResult();

                // Validar rol por device mode
                if (!ValidateRoleForDeviceMode(session.Role))
                {
                    var accessDenied = new Views.AccessDeniedView();
                    accessDenied.Initialize(signalR, session.BusinessId);
                    Content = accessDenied;
                    return;
                }

                // Cargar vista según device mode
                LoadViewForDeviceMode(api, signalR, session);
            };

            Content = start;
        }

        private static string GetBaseUrlFromConfigOrDefault(LocalDeviceConfig config)
        {
            // Si tu LocalDeviceConfig tiene otra propiedad, cámbiala aquí (ApiBaseUrl/BaseUrl/ServerUrl).
            // Si NO existe, cae a localhost.
            var url = TryReadStringProperty(config, "ApiBaseUrl")
                   ?? TryReadStringProperty(config, "BaseUrl")
                   ?? TryReadStringProperty(config, "ServerUrl")
                   ?? "https://localhost:5001/";

            url = url.Trim();
            if (!url.EndsWith("/")) url += "/";
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