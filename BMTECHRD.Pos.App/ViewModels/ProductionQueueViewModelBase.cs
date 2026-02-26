using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels;

public abstract class ProductionQueueViewModelBase : ViewModelBase
{
    protected readonly ApiClient _api;
    protected readonly SignalRClient _signalR;
    protected readonly Guid _businessId;

    public ObservableCollection<ProductionQueueItemModel> Items { get; } = new();

    private bool _isBusy;
    public new bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LoadingText));
        }
    }

    public new string LoadingText => IsBusy ? "Cargando..." : string.Empty;

    private string? _error;
    public new string? Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(ErrorVisible)); } }

    public new bool ErrorVisible => !string.IsNullOrEmpty(Error);

    private string _filter = "PENDING";
    public string Filter { get => _filter; set { _filter = value; OnPropertyChanged(); ApplyFilter(); } }

    public int PendingCount => Items.Count(i => string.Equals(i.Status, "SENT", StringComparison.OrdinalIgnoreCase));
    public int InProgressCount => Items.Count(i => string.Equals(i.Status, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase));

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SetFilterCommand { get; }
    public RelayCommand StartCommand { get; }
    public RelayCommand DoneCommand { get; }

    protected ProductionQueueViewModelBase(ApiClient api, SignalRClient signalR, Guid businessId)
    {
        _api = api;
        _signalR = signalR;
        _businessId = businessId;

        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        SetFilterCommand = new RelayCommand(p => { if (p is string s) Filter = s; });
        StartCommand = new RelayCommand(async p => await StartAsync(p));
        DoneCommand = new RelayCommand(async p => await DoneAsync(p));
    }

    protected abstract Task LoadItemsAsync();

    public async Task RefreshAsync()
    {
        IsBusy = true; Error = null;
        try
        {
            await LoadItemsAsync();
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(InProgressCount));
            ApplyFilter();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    private void ApplyFilter()
    {
        // UI binding uses Items collection directly; filtering can be applied in view via CollectionView if desired.
        OnPropertyChanged(nameof(Items));
    }

    protected virtual async Task StartAsync(object? param)
    {
        if (param is not ProductionQueueItemModel item) return;
        if (!string.Equals(item.Status, "SENT", StringComparison.OrdinalIgnoreCase)) return;
        var ok = await UpdateStatusAsync(item.OrderItemId, "IN_PROGRESS");
        if (ok) await RefreshAsync();
    }

    protected virtual async Task DoneAsync(object? param)
    {
        if (param is not ProductionQueueItemModel item) return;
        if (!string.Equals(item.Status, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase)) return;
        var ok = await UpdateStatusAsync(item.OrderItemId, "DONE");
        if (ok) await RefreshAsync();
    }

    protected abstract Task<bool> UpdateStatusAsync(Guid orderItemId, string status);
}
