using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels.Admin;

public sealed class ShiftsAdminViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId;

    public ObservableCollection<ShiftListItemModel> Shifts { get; } = new();
    public System.ComponentModel.ICollectionView ShiftsView { get; private set; }

    private ShiftListItemModel? _selectedShift;
    public ShiftListItemModel? SelectedShift
    {
        get => _selectedShift;
        set
        {
            if (_selectedShift == value) return;
            _selectedShift = value;
            OnPropertyChanged();
            if (value != null)
                _ = LoadSummaryAsync();
        }
    }

    private ShiftSummaryModel? _summary;
    public ShiftSummaryModel? Summary { get => _summary; set { _summary = value; OnPropertyChanged(); } }

    // Filters
    private Guid? _selectedUserId;
    public Guid? SelectedUserId { get => _selectedUserId; set { _selectedUserId = value; OnPropertyChanged(); } }

    private DateTime? _fromDate;
    public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

    private DateTime? _toDate;
    public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

    private string? _selectedStatus;
    public string? SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); } }

    private int _limit = 200;
    public int Limit { get => _limit; set { _limit = value; OnPropertyChanged(); } }

    public RelayCommand LoadShiftsCommand { get; }
    public RelayCommand LoadSummaryCommand { get; }
    public RelayCommand ClearSummaryCommand { get; }

    public ShiftsAdminViewModel(ApiClient api, Guid businessId, Guid actorUserId)
    {
        _api = api;
        _businessId = businessId;
        _actorUserId = actorUserId;

        ShiftsView = CollectionViewSource.GetDefaultView(Shifts);
        ShiftsView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ShiftListItemModel.OpenedAt), System.ComponentModel.ListSortDirection.Descending));

        LoadShiftsCommand = new RelayCommand(async _ => await LoadShiftsAsync());
        LoadSummaryCommand = new RelayCommand(async _ => await LoadSummaryAsync());
        ClearSummaryCommand = new RelayCommand(_ => { Summary = null; SelectedShift = null; });

        _ = LoadShiftsAsync();
    }

    public async Task LoadShiftsAsync()
    {
        IsBusy = true;
        Error = null;
        try
        {
            var list = await _api.GetShiftsAsync(_businessId, SelectedUserId, FromDate, ToDate, SelectedStatus, Limit);
            Shifts.Clear();
            foreach (var shift in list)
                Shifts.Add(shift);
            ShiftsView.Refresh();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
        }
    }

    public async Task LoadSummaryAsync()
    {
        if (SelectedShift == null)
        {
            Summary = null;
            return;
        }

        IsBusy = true;
        Error = null;
        try
        {
            var summary = await _api.GetShiftSummaryAsync(_businessId, SelectedShift.ShiftId);
            Summary = summary;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
        }
    }
}
