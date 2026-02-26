using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using System.Windows;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class StartViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly AuthSessionService _session;

    public ObservableCollection<BusinessPublicModel> Businesses { get; } = new();

    public BusinessPublicModel? SelectedBusiness { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public new bool IsBusy { get; set; }
    public new string? Error { get; set; }

    public RelayCommand LoadBusinessesCommand { get; }
    public RelayCommand LoginCommand { get; }

    public event System.Action<SessionModel>? OnLoginSuccess;

    public StartViewModel(ApiClient api, AuthSessionService session)
    {
        _api = api;
        _session = session ?? throw new System.ArgumentNullException(nameof(session));
        LoadBusinessesCommand = new RelayCommand(async _ => await LoadAsync());
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetBusinessesPublicAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Businesses.Clear();
                foreach (var b in list) Businesses.Add(b);
            });
        }
        finally { IsBusy = false; }
    }

    private async Task LoginAsync()
    {
        if (SelectedBusiness == null) { Error = "Select a business"; return; }
        IsBusy = true; Error = null;
        try
        {
            var req = new BMTECHRD.Pos.Application.DTOs.LoginRequest
            {
                BusinessId = SelectedBusiness.BusinessId,
                Username = Username,
                Password = Password,
                DeviceId = _session.DeviceId // ETAPA 9: enviar DeviceId en login
            };
            var resp = await _api.LoginAsync(req);
            if (resp == null) { Error = "Invalid credentials"; return; }

            // Incluir tokens en sesión (BLOQUE 7)
            var session = new SessionModel 
            { 
                BusinessId = resp.BusinessId, 
                UserId = resp.UserId, 
                Username = resp.Username, 
                Role = resp.Role,
                AccessToken = resp.AccessToken,
                RefreshToken = resp.RefreshToken,
                ExpiresAt = resp.ExpiresAt
            };
            OnLoginSuccess?.Invoke(session);
        }
        finally { IsBusy = false; }
    }
}
