using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels.Admin;

public sealed class UsersAdminViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId;

    public ObservableCollection<UserListItemModel> Users { get; } = new();
    public System.ComponentModel.ICollectionView UsersView { get; private set; }

    private UserListItemModel? _selectedUser;
    public UserListItemModel? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
            if (value != null)
            {
                EditRole = value.Role;
                EditIsActive = value.IsActive;
            }
            UpdateCommandsCanExecute();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value ?? string.Empty;
            OnPropertyChanged();
            UsersView?.Refresh();
        }
    }

    public new bool IsBusy { get; set; }
    public new string? Error { get; set; }
    public string? SuccessMessage { get; set; }

    // create
    public string NewUsername { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string NewPin4 { get; set; } = string.Empty;
    public string NewRole { get; set; } = "WAITER";
    public bool NewIsActive { get; set; } = true;

    // edit
    public string EditRole { get; set; } = "WAITER";
    public bool EditIsActive { get; set; }

    // reset
    public string ResetPasswordValue { get; set; } = string.Empty;
    public string ResetPinValue { get; set; } = string.Empty;

    public RelayCommand LoadCommand { get; }
    public RelayCommand CreateUserCommand { get; }
    public RelayCommand UpdateUserCommand { get; }
    public RelayCommand ResetPasswordCommand { get; }
    public RelayCommand ResetPinCommand { get; }

    public UsersAdminViewModel(ApiClient api, Guid businessId, Guid actorUserId)
    {
        _api = api;
        _businessId = businessId;
        _actorUserId = actorUserId;
        UsersView = System.Windows.Data.CollectionViewSource.GetDefaultView(Users);
        UsersView.Filter = FilterUser;

        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        CreateUserCommand = new RelayCommand(async _ => await CreateUserAsync());
        UpdateUserCommand = new RelayCommand(async _ => await UpdateUserAsync(), _ => SelectedUser != null);
        ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync(), _ => SelectedUser != null);
        ResetPinCommand = new RelayCommand(async _ => await ResetPinAsync(), _ => SelectedUser != null);

        _ = LoadAsync();
    }

    public string[] Roles => new[] { "ADMIN", "SUPERVISOR", "CASHIER", "WAITER", "KITCHEN", "BAR" };

    public async Task LoadAsync()
    {
        IsBusy = true; Error = null;
        try
        {
            var list = await _api.GetUsersAsync(_businessId);
            Users.Clear();
            foreach (var u in list) Users.Add(u);
            UsersView.Refresh();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    private bool FilterUser(object? obj)
    {
        if (obj is not UserListItemModel u) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) || u.Role.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateCommandsCanExecute()
    {
        UpdateUserCommand?.RaiseCanExecuteChanged();
        ResetPasswordCommand?.RaiseCanExecuteChanged();
        ResetPinCommand?.RaiseCanExecuteChanged();
    }

    public async Task CreateUserAsync()
    {
        Error = null; SuccessMessage = null;
        if (string.IsNullOrWhiteSpace(NewUsername)) { Error = "Username required"; return; }
        if (string.IsNullOrWhiteSpace(NewPassword)) { Error = "Password required"; return; }
        if (!string.IsNullOrEmpty(NewPin4) && (NewPin4.Length < 4 || NewPin4.Length > 12 || !NewPin4.All(char.IsDigit))) { Error = "PIN must be numeric with 4 to 12 digits"; return; }
        if (NewPassword.Length < 4 || NewPassword.Length > 12) { Error = "Password must contain between 4 and 12 characters"; return; }

        IsBusy = true; OnPropertyChanged(nameof(IsBusy));
        try
        {
            var req = new CreateUserRequestModel { BusinessId = _businessId, ActorUserId = _actorUserId, Username = NewUsername, Password = NewPassword, Pin4 = string.IsNullOrEmpty(NewPin4) ? null : NewPin4, Role = NewRole, IsActive = NewIsActive };
            var ok = await _api.CreateUserAsync(req);
            if (!ok) { Error = "Create failed"; return; }
            SuccessMessage = "User created";
            await LoadAsync();
            NewUsername = NewPassword = NewPin4 = string.Empty;
            OnPropertyChanged(nameof(NewUsername)); OnPropertyChanged(nameof(NewPassword)); OnPropertyChanged(nameof(NewPin4));
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    public async Task UpdateUserAsync()
    {
        if (SelectedUser == null) return;
        IsBusy = true; Error = null;
        try
        {
            var req = new UpdateUserRequestModel { BusinessId = _businessId, ActorUserId = _actorUserId, Role = EditRole, IsActive = EditIsActive };
            var ok = await _api.UpdateUserAsync(SelectedUser.Id, req);
            if (!ok) { Error = "Update failed"; return; }
            await LoadAsync();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    public async Task ResetPasswordAsync()
    {
        if (SelectedUser == null) return;
        if (string.IsNullOrWhiteSpace(ResetPasswordValue)) { Error = "New password required"; return; }
        if (ResetPasswordValue.Length < 4 || ResetPasswordValue.Length > 12) { Error = "Password must contain between 4 and 12 characters"; return; }
        IsBusy = true; Error = null;
        try
        {
            var req = new ResetPasswordRequestModel { BusinessId = _businessId, ActorUserId = _actorUserId, NewPassword = ResetPasswordValue };
            var ok = await _api.ResetPasswordAsync(SelectedUser.Id, req);
            if (!ok) { Error = "Reset password failed"; return; }
            SuccessMessage = "Password reset";
            ResetPasswordValue = string.Empty; OnPropertyChanged(nameof(ResetPasswordValue));
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    public async Task ResetPinAsync()
    {
        if (SelectedUser == null) return;
        if (string.IsNullOrWhiteSpace(ResetPinValue) || ResetPinValue.Length < 4 || ResetPinValue.Length > 12 || !ResetPinValue.All(char.IsDigit)) { Error = "New PIN must be numeric with 4 to 12 digits"; return; }
        IsBusy = true; Error = null;
        try
        {
            var req = new ResetPinRequestModel { BusinessId = _businessId, ActorUserId = _actorUserId, NewPin4 = ResetPinValue };
            var ok = await _api.ResetPinAsync(SelectedUser.Id, req);
            if (!ok) { Error = "Reset PIN failed"; return; }
            SuccessMessage = "PIN reset";
            ResetPinValue = string.Empty; OnPropertyChanged(nameof(ResetPinValue));
            await LoadAsync();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }
}
