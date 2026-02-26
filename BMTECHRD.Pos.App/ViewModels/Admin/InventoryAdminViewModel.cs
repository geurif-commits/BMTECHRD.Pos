using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels.Admin;

public sealed class InventoryAdminViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;

    public ObservableCollection<StockItemModel> Stock { get; } = new();
    public System.ComponentModel.ICollectionView StockView { get; private set; }

    private StockItemModel? _selectedProduct;
    public StockItemModel? SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged(); } }

    public ObservableCollection<InventoryMovementModel> Movements { get; } = new();

    public new bool IsBusy { get; set; }
    public new string? Error { get; set; }

    public string StockSearchText { get; set; } = string.Empty;
    public int Limit { get; set; } = 200;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int AdjustDelta { get; set; }
    public string AdjustReason { get; set; } = "AJUSTE";

    public RelayCommand LoadStockCommand { get; }
    public RelayCommand LoadMovementsCommand { get; }
    public RelayCommand ApplyAdjustCommand { get; }
    public RelayCommand AdjustByCommand { get; }

    public InventoryAdminViewModel(ApiClient api, Guid businessId)
    {
        _api = api;
        _businessId = businessId;

        StockView = System.Windows.Data.CollectionViewSource.GetDefaultView(Stock);
        StockView.Filter = o => string.IsNullOrWhiteSpace(StockSearchText) || ((StockItemModel)o).Name.Contains(StockSearchText, StringComparison.OrdinalIgnoreCase);

        LoadStockCommand = new RelayCommand(async _ => await LoadStockAsync());
        LoadMovementsCommand = new RelayCommand(async _ => await LoadMovementsAsync());
        ApplyAdjustCommand = new RelayCommand(async _ => await ApplyAdjustAsync());
        AdjustByCommand = new RelayCommand(param =>
        {
            if (param is int delta)
            {
                AdjustDelta += delta;
                OnPropertyChanged(nameof(AdjustDelta));
            }
        });

        _ = LoadStockAsync();
    }

    public async Task LoadStockAsync()
    {
        IsBusy = true; Error = null;
        try
        {
            var list = await _api.GetStockAsync(_businessId);
            Stock.Clear();
            foreach (var s in list) Stock.Add(s);
            StockView.Refresh();
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    public async Task LoadMovementsAsync()
    {
        IsBusy = true; Error = null;
        try
        {
            var productId = SelectedProduct?.ProductId;
            var from = FromDate;
            var to = ToDate;
            var take = Limit;
            if (take <= 0) take = 200;
            if (take > 1000) take = 1000;
            var list = await _api.GetMovementsAsync(_businessId, productId, from, to, take);
            Movements.Clear();
            foreach (var m in list) Movements.Add(m);
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }

    public async Task ApplyAdjustAsync()
    {
        Error = null;
        if (SelectedProduct == null) { Error = "Seleccione un producto"; return; }
        if (AdjustDelta == 0) { Error = "Delta must be non-zero"; return; }
        if (string.IsNullOrWhiteSpace(AdjustReason)) { Error = "Reason required"; return; }

        IsBusy = true; OnPropertyChanged(nameof(IsBusy));
        try
        {
            var req = new InventoryAdjustRequestModel { BusinessId = _businessId, ProductId = SelectedProduct.ProductId, QuantityDelta = AdjustDelta, Reason = AdjustReason };
            var ok = await _api.AdjustInventoryAsync(req);
            if (!ok) { Error = "Adjust failed"; return; }
            await LoadStockAsync();
            await LoadMovementsAsync();
            AdjustDelta = 0; AdjustReason = "AJUSTE"; OnPropertyChanged(nameof(AdjustDelta)); OnPropertyChanged(nameof(AdjustReason));
        }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(IsBusy)); }
    }
}
