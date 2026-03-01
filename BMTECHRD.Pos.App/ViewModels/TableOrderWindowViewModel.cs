using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class TableOrderWindowViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _tableId;
    private readonly Guid _actorUserId;
    private readonly Action<IReadOnlyCollection<TableOrderLineModel>> _persistDraft;

    private MenuCategoryModel? _selectedCategory;
    private string _search = string.Empty;

    public ObservableCollection<MenuCategoryModel> Categories { get; } = new();
    public ObservableCollection<MenuProductModel> Products { get; } = new();
    public ObservableCollection<MenuProductModel> FilteredProducts { get; } = new();
    public ObservableCollection<TableOrderLineModel> Cart { get; } = new();

    public string HeaderTitle { get; }

    public MenuCategoryModel? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public string Search
    {
        get => _search;
        set
        {
            _search = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public decimal Total => Cart.Sum(x => x.LineTotal);

    public RelayCommand AddProductCommand { get; }
    public RelayCommand IncreaseLineCommand { get; }
    public RelayCommand DecreaseLineCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public RelayCommand SendOrderCommand { get; }

    public event Action? OrderSent;

    public TableOrderWindowViewModel(
        ApiClient api,
        Guid businessId,
        Guid tableId,
        Guid actorUserId,
        int tableNumber,
        IReadOnlyCollection<TableOrderLineModel> initialDraft,
        Action<IReadOnlyCollection<TableOrderLineModel>> persistDraft)
    {
        _api = api;
        _businessId = businessId;
        _tableId = tableId;
        _actorUserId = actorUserId;
        _persistDraft = persistDraft;

        HeaderTitle = $"Mesa {tableNumber} · Comanda digital";

        AddProductCommand = new RelayCommand(p => AddProduct(p as MenuProductModel));
        IncreaseLineCommand = new RelayCommand(p => IncreaseLine(p as TableOrderLineModel));
        DecreaseLineCommand = new RelayCommand(p => DecreaseLine(p as TableOrderLineModel));
        RemoveLineCommand = new RelayCommand(p => RemoveLine(p as TableOrderLineModel));
        SendOrderCommand = new RelayCommand(async _ => await SendAsync());

        foreach (var draft in initialDraft)
        {
            var line = new TableOrderLineModel
            {
                ProductId = draft.ProductId,
                ProductName = draft.ProductName,
                Quantity = draft.Quantity,
                UnitPrice = draft.UnitPrice,
                Area = draft.Area
            };
            line.PropertyChanged += CartLineOnPropertyChanged;
            Cart.Add(line);
        }

        Cart.CollectionChanged += CartOnCollectionChanged;
    }

    public async Task InitializeAsync()
    {
        var categories = await _api.GetCategoriesAsync(_businessId);
        var products = await _api.GetProductsAsync(_businessId);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Categories.Clear();
            Categories.Add(new MenuCategoryModel { Id = Guid.Empty, Name = "Todos", IsActive = true, SortOrder = -1 });

            foreach (var category in categories.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
            {
                Categories.Add(category);
            }

            Products.Clear();
            foreach (var product in products.Where(x => x.IsActive).OrderBy(x => x.Name))
            {
                Products.Add(product);
            }

            SelectedCategory = Categories.FirstOrDefault();
            ApplyFilter();
            OnPropertyChanged(nameof(Total));
        });
    }

    private void AddProduct(MenuProductModel? product)
    {
        if (product == null) return;

        var line = Cart.FirstOrDefault(x => x.ProductId == product.Id);
        if (line == null)
        {
            line = new TableOrderLineModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = 1,
                UnitPrice = product.Price,
                Area = product.Area
            };

            line.PropertyChanged += CartLineOnPropertyChanged;
            Cart.Add(line);
        }
        else
        {
            line.Quantity += 1;
        }

        PersistDraft();
        OnPropertyChanged(nameof(Total));
    }


    private void IncreaseLine(TableOrderLineModel? line)
    {
        if (line == null) return;
        line.Quantity += 1;
        PersistDraft();
        OnPropertyChanged(nameof(Total));
    }

    private void DecreaseLine(TableOrderLineModel? line)
    {
        if (line == null) return;

        if (line.Quantity <= 1)
        {
            RemoveLine(line);
            return;
        }

        line.Quantity -= 1;
        PersistDraft();
        OnPropertyChanged(nameof(Total));
    }

    private void RemoveLine(TableOrderLineModel? line)
    {
        if (line == null) return;

        line.PropertyChanged -= CartLineOnPropertyChanged;
        Cart.Remove(line);
        PersistDraft();
        OnPropertyChanged(nameof(Total));
    }

    private async Task SendAsync()
    {
        if (!Cart.Any())
        {
            MessageBox.Show("Agrega al menos un producto antes de enviar la comanda.", "Comanda", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var request = new CreateOrderBatchRequest
        {
            BusinessId = _businessId,
            TableId = _tableId,
            ActorUserId = _actorUserId,
            Items = Cart.Select(x => new CreateOrderBatchLine
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToList()
        };

        var response = await _api.CreateOrderBatchAsync(request);
        if (response == null)
        {
            MessageBox.Show("No se pudo enviar la comanda. Verifica inventario y estado de la mesa.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show($"Comanda enviada. Cocina: {response.KitchenItems}, Bar: {response.BarItems}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

        foreach (var line in Cart)
        {
            line.PropertyChanged -= CartLineOnPropertyChanged;
        }

        Cart.Clear();
        PersistDraft();
        OnPropertyChanged(nameof(Total));
        OrderSent?.Invoke();
    }

    private void ApplyFilter()
    {
        var selectedCategoryId = SelectedCategory?.Id;
        var term = Search?.Trim() ?? string.Empty;

        var query = Products.AsEnumerable();
        if (selectedCategoryId.HasValue && selectedCategoryId.Value != Guid.Empty)
        {
            query = query.Where(x => x.CategoryId == selectedCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        FilteredProducts.Clear();
        foreach (var product in query)
        {
            FilteredProducts.Add(product);
        }
    }

    private void CartOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PersistDraft();
        OnPropertyChanged(nameof(Total));
    }

    private void CartLineOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TableOrderLineModel.Quantity))
        {
            PersistDraft();
            OnPropertyChanged(nameof(Total));
        }
    }

    private void PersistDraft()
    {
        _persistDraft(Cart.Select(x => new TableOrderLineModel
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Area = x.Area
        }).ToList());
    }
}
