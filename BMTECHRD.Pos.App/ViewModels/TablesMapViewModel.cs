using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class TablesMapViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId;
    private readonly Dictionary<Guid, List<TableOrderLineModel>> _tableDraftOrders = new();

    public ObservableCollection<TableModel> Tables { get; } = new();

    public int AvailableTables => Tables.Count(x => string.Equals(x.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase));
    public int OpenTables => Tables.Count(x => string.Equals(x.Status, "OPEN", StringComparison.OrdinalIgnoreCase));

    public RelayCommand LoadTablesCommand { get; }
    public RelayCommand OpenOrAccessTableCommand { get; }
    public RelayCommand UpdatePositionCommand { get; }

    public TablesMapViewModel(ApiClient api, Guid businessId, Guid actorUserId)
    {
        _api = api;
        _businessId = businessId;
        _actorUserId = actorUserId;

        LoadTablesCommand = new RelayCommand(async _ => await LoadTablesAsync());
        OpenOrAccessTableCommand = new RelayCommand(async param => await OpenOrAccessAsync(param));
        UpdatePositionCommand = new RelayCommand(async param => await UpdatePositionAsync(param));
    }

    public async Task LoadTablesAsync()
    {
        try
        {
            var list = await _api.GetTablesAsync(_businessId);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Tables.Clear();
                foreach (var t in list.OrderBy(x => x.Number)) Tables.Add(t);
                OnPropertyChanged(nameof(AvailableTables));
                OnPropertyChanged(nameof(OpenTables));
            });
        }
        catch
        {
            MessageBox.Show("No se pudo cargar el mapa de mesas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task OpenOrAccessAsync(object? param)
    {
        if (param is not TableModel table) return;

        if (string.Equals(table.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            var openResponse = await _api.OpenTableAsync(table.Id, _actorUserId);
            if (!openResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("No se pudo abrir la mesa.", "Comanda", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadTablesAsync();
        }

        var access = await _api.AccessTableAsync(table.Id, _actorUserId, string.Empty);
        if (access == null || (access.RequiresPin && !access.AccessGranted))
        {
            var pinWindow = new BMTECHRD.Pos.App.Windows.PinWindow();
            var ok = pinWindow.ShowDialog();
            if (ok != true) return;

            var pin = pinWindow.EnteredPin ?? string.Empty;
            var pinAccess = await _api.AccessTableAsync(table.Id, _actorUserId, pin);
            if (pinAccess == null || !pinAccess.AccessGranted)
            {
                MessageBox.Show("PIN inválido para esta mesa.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else if (!access.AccessGranted)
        {
            MessageBox.Show("No tienes permisos para esta mesa.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OpenOrderWindow(table);
    }

    private void OpenOrderWindow(TableModel table)
    {
        if (!_tableDraftOrders.TryGetValue(table.Id, out var draftLines))
        {
            draftLines = new List<TableOrderLineModel>();
            _tableDraftOrders[table.Id] = draftLines;
        }

        var orderWindow = new BMTECHRD.Pos.App.Views.TableOrderWindow(
            _api,
            _businessId,
            table.Id,
            _actorUserId,
            table.Number,
            draftLines,
            updated => _tableDraftOrders[table.Id] = updated.ToList())
        {
            Owner = Application.Current.MainWindow
        };

        orderWindow.ShowDialog();
    }

    private async Task UpdatePositionAsync(object? param)
    {
        if (param is not TableModel table) return;
        await _api.UpdateTablePositionAsync(table.Id, table.PosX, table.PosY);
    }
}
