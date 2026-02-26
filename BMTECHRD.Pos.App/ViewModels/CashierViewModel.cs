using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class CashierViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SignalRClient _signalR;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId;

    public ObservableCollection<TableSummaryModel> Tables { get; } = new();
    public ObservableCollection<BillLineModel> Lines { get; } = new();

    private TableSummaryModel? _selectedTable;
    public TableSummaryModel? SelectedTable
    {
        get => _selectedTable;
        set
        {
            _selectedTable = value;
            OnPropertyChanged();
            _ = LoadBillAsync();
            RegisterPaymentCommand?.RaiseCanExecuteChanged();
        }
    }

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Tip { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }

    public bool HasPendingItems { get; set; }
    public string? CloseBlockedReason { get; set; }

    // Shift properties
    private Guid? _activeShiftId;
    public Guid? ActiveShiftId { get => _activeShiftId; set { _activeShiftId = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasActiveShift)); RegisterPaymentCommand?.RaiseCanExecuteChanged(); } }
    public bool HasActiveShift => ActiveShiftId.HasValue;
    public string? ShiftLabel { get; set; }

    // Payment UI properties
    public string SelectedPaymentMethod { get; set; } = "CASH";
    private decimal _paymentAmount;
    public decimal PaymentAmount { get => _paymentAmount; set { _paymentAmount = value; OnPropertyChanged(); RegisterPaymentCommand?.RaiseCanExecuteChanged(); } }
    private decimal? _cashGiven;
    public decimal? CashGiven { get => _cashGiven; set { _cashGiven = value; OnPropertyChanged(); } }
    private decimal _change;
    public decimal Change { get => _change; set { _change = value; OnPropertyChanged(); } }

    public RelayCommand LoadCommand { get; }
    public RelayCommand RefreshTablesCommand => LoadCommand; // alias for bindings

    public RelayCommand PayCashCommand { get; }
    public RelayCommand PayCardCommand { get; }
    public RelayCommand RegisterPaymentCommand { get; }
    public RelayCommand SetPaymentMethodCommand { get; }
    public RelayCommand LoadShiftCommand { get; }
    public RelayCommand OpenShiftCommand { get; }
    public RelayCommand CloseShiftCommand { get; }

    public CashierViewModel(ApiClient api, SignalRClient signalR, Guid businessId, Guid actorUserId)
    {
        _api = api;
        _signalR = signalR;
        _businessId = businessId;
        _actorUserId = actorUserId;

        LoadCommand = new RelayCommand(async _ => await RefreshAsync());
        PayCashCommand = new RelayCommand(async p => await PayAsync("CASH", p));
        PayCardCommand = new RelayCommand(async p => await PayAsync("CARD", p));
        RegisterPaymentCommand = new RelayCommand(async _ => await PayAsync(SelectedPaymentMethod, null), _ => HasActiveShift && SelectedTable != null && !IsBusy && (PaymentAmount > 0 || Due > 0));
        SetPaymentMethodCommand = new RelayCommand(p => { if (p is string s) SelectedPaymentMethod = s; });
        LoadShiftCommand = new RelayCommand(async _ => await LoadShiftAsync());
        OpenShiftCommand = new RelayCommand(async p => await OpenShiftAsync());
        CloseShiftCommand = new RelayCommand(async p => await CloseShiftAsync());

        _signalR.OnTablesUpdated += async () => await RefreshAsync();
        _signalR.OnInventoryUpdated += async () => await RefreshAsync();
        _signalR.OnTablesUpdated += async () => await RefreshAsync();

        // load current shift on startup
        _ = LoadShiftAsync();
    }

    private async Task LoadShiftAsync()
    {
        try
        {
            var resp = await _api.GetActiveShiftAsync(_businessId, _actorUserId);
            if (resp == null || resp.Status == "NONE")
            {
                ActiveShiftId = null;
                ShiftLabel = null;
            }
            else
            {
                ActiveShiftId = resp.ShiftId;
                ShiftLabel = $"Turno abierto {resp.OpenedAt:HH:mm} - {resp.OpenedByUsername}";
            }
            OnPropertyChanged(nameof(ShiftLabel));
        }
        catch { /* ignore */ }
    }

    private async Task OpenShiftAsync()
    {
        try
        {
            IsBusy = true; Error = null;
            var req = new CreateShiftRequest { BusinessId = _businessId, UserId = _actorUserId, OpeningCash = 0m };
            var resp = await _api.OpenShiftAsync(req);
            if (resp != null)
            {
                ActiveShiftId = resp.ShiftId;
                ShiftLabel = $"Turno abierto {resp.OpenedAt:HH:mm}";
                OnPropertyChanged(nameof(ShiftLabel));
            }
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task CloseShiftAsync()
    {
        if (!ActiveShiftId.HasValue) return;
        try
        {
            IsBusy = true; Error = null;
            var req = new CloseShiftRequest { BusinessId = _businessId, UserId = _actorUserId, ShiftId = ActiveShiftId.Value, ClosingCash = 0m };
            var resp = await _api.CloseShiftAsync(req);
            // clear
            ActiveShiftId = null;
            ShiftLabel = null;
            OnPropertyChanged(nameof(ShiftLabel));
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    public async Task RefreshAsync()
    {
        IsBusy = true; Error = null;
        try
        {
            var list = await _api.GetCashTablesAsync(_businessId);
            Tables.Clear();
            foreach (var t in list) Tables.Add(new TableSummaryModel { TableId = t.TableId, TableNumber = t.TableNumber, Status = t.Status, CurrentTotal = t.CurrentTotal, ItemsCount = t.ItemsCount });
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task LoadBillAsync()
    {
        if (SelectedTable == null) return;
        IsBusy = true; Error = null;
        try
        {
            var resp = await _api.GetBillAsync(_businessId, SelectedTable.TableId);
            if (resp == null) { Error = "Could not load bill"; return; }
            Lines.Clear();
            foreach (var l in resp.Lines) Lines.Add(new BillLineModel { ProductId = l.ProductId, Name = l.Name, Quantity = l.Quantity, UnitPrice = l.UnitPrice, LineTotal = l.LineTotal, Area = l.Area, Status = l.Status });
            Subtotal = resp.Subtotal; Tax = resp.Tax; Tip = resp.Tip; Discount = resp.Discount; Total = resp.Total; Paid = resp.Paid; Due = resp.Due;
            HasPendingItems = resp.HasPendingItems;
            OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Tax)); OnPropertyChanged(nameof(Tip)); OnPropertyChanged(nameof(Discount)); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(Paid)); OnPropertyChanged(nameof(Due));
            OnPropertyChanged(nameof(HasPendingItems));
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task PayAsync(string method, object? parameter)
    {
        if (SelectedTable == null) { Error = "Select a table"; return; }
        IsBusy = true; Error = null;
        try
        {
            decimal amount = 0;
            decimal? cashGiven = null;
            if (parameter is decimal d) amount = d;
            else if (PaymentAmount > 0) amount = PaymentAmount;
            else amount = Due;

            cashGiven = CashGiven;

            var req = new CreatePaymentRequest { BusinessId = _businessId, TableId = SelectedTable.TableId, ShiftId = ActiveShiftId ?? Guid.Empty, ActorUserId = _actorUserId, Method = method, Amount = amount, CashGiven = cashGiven, CloseIfPaid = true };
            var resp = await _api.CreatePaymentAsync(req);
            if (resp == null) { Error = "Payment failed"; return; }
            Paid = resp.Paid; Due = resp.Due;
            CloseBlockedReason = resp.CloseBlockedReason;
            OnPropertyChanged(nameof(CloseBlockedReason));
            // compute change if cashGiven provided
            if (cashGiven.HasValue)
            {
                Change = Math.Max(0, cashGiven.Value - amount);
                OnPropertyChanged(nameof(Change));
            }
            OnPropertyChanged(nameof(Paid)); OnPropertyChanged(nameof(Due));
            if (resp.Closed)
            {
                // refresh tables
                await RefreshAsync();
                SelectedTable = null;
                Lines.Clear();
            }
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }
}
