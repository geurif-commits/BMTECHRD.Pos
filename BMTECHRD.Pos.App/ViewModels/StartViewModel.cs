using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class StartViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly QuickLoginProfileService _quickLoginProfileService;

    public ObservableCollection<BusinessPublicModel> Businesses { get; } = new();

    public BusinessPublicModel? SelectedBusiness { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool HasQuickLogin => RememberedProfile != null;
    public string QuickLoginLabel => RememberedProfile == null
        ? string.Empty
        : $"Inicio rápido habilitado: {RememberedProfile.Username} ({RememberedProfile.BusinessName})";

    public QuickLoginProfile? RememberedProfile { get; private set; }

    public RelayCommand LoadBusinessesCommand { get; }
    public RelayCommand LoginCommand { get; }
    public RelayCommand ClearQuickLoginCommand { get; }

    public event System.Action<SessionModel>? OnLoginSuccess;

    public StartViewModel(ApiClient api)
    {
        _api = api;
        _quickLoginProfileService = new QuickLoginProfileService();
        RememberedProfile = _quickLoginProfileService.Load();

        LoadBusinessesCommand = new RelayCommand(async _ => await LoadAsync());
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
        ClearQuickLoginCommand = new RelayCommand(_ => ClearQuickLogin());
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        Error = null;
        try
        {
            var list = await _api.GetBusinessesPublicAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Businesses.Clear();
                foreach (var b in list) Businesses.Add(b);

                if (RememberedProfile != null)
                {
                    SelectedBusiness = Businesses.FirstOrDefault(x => x.BusinessId == RememberedProfile.BusinessId);
                    OnPropertyChanged(nameof(SelectedBusiness));
                }

                OnPropertyChanged(nameof(QuickLoginLabel));
                OnPropertyChanged(nameof(HasQuickLogin));
            });
        }
        catch
        {
            Error = "No se pudo cargar la lista de negocios.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoginAsync()
    {
        if (SelectedBusiness == null)
        {
            Error = "Select a business";
            return;
        }

        var usernameToUse = !string.IsNullOrWhiteSpace(Username)
            ? Username.Trim()
            : RememberedProfile?.Username;

        if (string.IsNullOrWhiteSpace(usernameToUse))
        {
            Error = "Username required for first login";
            return;
        }

        Error = null;

        var req = new BMTECHRD.Pos.Application.DTOs.LoginRequest
        {
            BusinessId = SelectedBusiness.BusinessId,
            Username = usernameToUse,
            Password = Password
        };

        var resp = await _api.LoginAsync(req);
        if (resp == null)
        {
            Error = "Invalid credentials";
            return;
        }

        _quickLoginProfileService.Save(new QuickLoginProfile
        {
            BusinessId = resp.BusinessId,
            BusinessName = SelectedBusiness.Name,
            Username = resp.Username
        });

        RememberedProfile = _quickLoginProfileService.Load();
        OnPropertyChanged(nameof(RememberedProfile));
        OnPropertyChanged(nameof(QuickLoginLabel));
        OnPropertyChanged(nameof(HasQuickLogin));

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

    private void ClearQuickLogin()
    {
        _quickLoginProfileService.Clear();
        RememberedProfile = null;
        OnPropertyChanged(nameof(RememberedProfile));
        OnPropertyChanged(nameof(HasQuickLogin));
        OnPropertyChanged(nameof(QuickLoginLabel));
    }
}
