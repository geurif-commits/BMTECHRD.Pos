using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Core;
using Microsoft.Extensions.DependencyInjection;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Auth.Core.Services;

namespace BMTECHRD.Pos.App
{
    public partial class MainWindow : Window
    {
        private DeviceMode _deviceMode;
        private readonly LocalDeviceConfigService _configService;
        private readonly IDeviceRolePolicy _deviceRolePolicy;
        private readonly IMainWindowSessionOrchestrator _sessionOrchestrator;
        private AuthSessionService? _authSession;
        private SignalRClient? _signalR;

        private void OnSessionExpired(object? sender, EventArgs e)
        {
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
                }

                var api = App.Services.GetService<ApiClient>();
                var start = new Views.StartView();
                start.Initialize(api!, _authSession!);
                Content = start;
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            _configService = new LocalDeviceConfigService();
            _deviceRolePolicy = App.Services.GetService<IDeviceRolePolicy>() ?? new DeviceRolePolicy();
            _sessionOrchestrator = App.Services.GetService<IMainWindowSessionOrchestrator>() ?? new MainWindowSessionOrchestrator();
        }

        public void SetContent(object content)
        {
            if (content is UIElement el)
                MainContent.Content = el;
            else
                MainContent.Content = content;
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

            _authSession = (AuthSessionService?)App.Services.GetService(typeof(AuthSessionService));
            if (_authSession == null)
            {
                MessageBox.Show("Error inicializando la sesión de autenticación. Reinicia la aplicación.", "Error DI", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var baseUrl = GetBaseUrlFromConfigOrDefault(config);
            var api = (ApiClient?)App.Services.GetService(typeof(ApiClient));
            if (api == null)
            {
                MessageBox.Show("Error inicializando el cliente API. Revisa la configuración y reinicia.", "Error DI", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var start = new Views.StartView();
            start.Initialize(api, _authSession);

            start.OnLoginSuccess += session =>
            {
                _authSession!.SetSession(session.AccessToken, session.RefreshToken, session.UserId, session.BusinessId, session.Username, session.Role, session.ExpiresAt);

                _authSession.SessionExpired -= OnSessionExpired;
                _authSession.SessionExpired += OnSessionExpired;

                Task.Run(async () =>
                {
                    _signalR = await _sessionOrchestrator.ConnectSignalRAsync(baseUrl, () => _authSession!.AccessToken, session.BusinessId);
                    if (_signalR == null) return;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_deviceRolePolicy.IsRoleAllowedForDeviceMode(_deviceMode, session.Role))
                        {
                            var accessDenied = new Views.AccessDeniedView();
                            accessDenied.Initialize(_signalR, session.BusinessId);
                            Content = accessDenied;
                            return;
                        }

                        Content = _sessionOrchestrator.BuildContentForDeviceMode(_deviceMode, api, _signalR, session);
                    });
                });
            };

            Content = start;
        }

        private static string GetBaseUrlFromConfigOrDefault(DeviceConfig config)
        {
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
    }
}
