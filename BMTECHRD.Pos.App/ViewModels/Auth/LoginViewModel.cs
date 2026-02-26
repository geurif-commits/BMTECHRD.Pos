using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BMTECHRD.Pos.App.ViewModels.Auth;

public sealed class LoginViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _api;
    private readonly AuthSessionService _session;
    private readonly INavigationService _nav;

    private string _businessIdText = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

    private bool _isBusy;
    private string? _error;
    private string? _status;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LoginViewModel(ApiClient api, AuthSessionService session, INavigationService nav)
    {
        _api = api;
        _session = session;
        _nav = nav;

        LoginCommand = new AsyncRelayCommand(LoginAsync, () => CanLogin);
        DeviceIdHint = $"DeviceId: {_session.DeviceId}";
        LoginButtonText = "Entrar";
    }

    public string BusinessIdText
    {
        get => _businessIdText;
        set { _businessIdText = value; OnChanged(); OnChanged(nameof(CanLogin)); ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    public string Username
    {
        get => _username;
        set { _username = value; OnChanged(); OnChanged(nameof(CanLogin)); ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    public string DeviceIdHint { get; }
    public string LoginButtonText { get; }

    public string? Error
    {
        get => _error;
        private set { _error = value; OnChanged(); }
    }

    public string? Status
    {
        get => _status;
        private set { _status = value; OnChanged(); }
    }

    public bool CanLogin =>
        !_isBusy &&
        Guid.TryParse(_businessIdText, out _) &&
        !string.IsNullOrWhiteSpace(_username) &&
        !string.IsNullOrWhiteSpace(_password);

    public ICommand LoginCommand { get; }

    public void SetPassword(string pwd)
    {
        _password = pwd ?? string.Empty;
        OnChanged(nameof(CanLogin));
        ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
    }

    private async Task LoginAsync()
    {
        Error = null;
        Status = null;

        if (!Guid.TryParse(BusinessIdText, out var businessId))
        {
            Error = "BusinessId inválido.";
            return;
        }

        _isBusy = true;
        OnChanged(nameof(CanLogin));
        ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();

        try
        {
            Status = "Autenticando...";

            var resp = await _api.LoginAsync(new LoginRequest
            {
                BusinessId = businessId,
                Username = Username.Trim(),
                Password = _password,
                DeviceId = _session.DeviceId
            });

            if (resp == null)
            {
                Error = "Credenciales inválidas o no autorizado.";
                Status = null;
                return;
            }

            _session.SetSession(
                resp.AccessToken,
                resp.RefreshToken,
                resp.UserId,
                resp.BusinessId,
                resp.Username,
                resp.Role,
                resp.ExpiresAt
            );

            Status = "Acceso concedido. Cargando...";
            await _nav.GoToShellAsync();
        }
        catch (Exception ex)
        {
            Error = $"Error al iniciar sesión: {ex.Message}";
            Status = null;
        }
        finally
        {
            _isBusy = false;
            OnChanged(nameof(CanLogin));
            ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    private void OnChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}