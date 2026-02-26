using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.Application.DTOs;

namespace BMTECHRD.Pos.App.ViewModels;

public sealed class TablesMapViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId; // current user

    public ObservableCollection<TableModel> Tables { get; } = new();

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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Tables.Clear();
                foreach (var t in list.OrderBy(x => x.Number)) Tables.Add(t);
            });
        }
        catch { /* ignore for now */ }
    }

    private async Task OpenOrAccessAsync(object? param)
    {
        if (param is not TableModel table) return;

        if (string.Equals(table.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            await _api.OpenTableAsync(table.Id, _actorUserId);
            await LoadTablesAsync();
            return;
        }

        // if OPEN, call access endpoint, maybe prompt for PIN
        var access = await _api.AccessTableAsync(table.Id, _actorUserId, string.Empty);
        if (access == null || (access.RequiresPin && !access.AccessGranted))
        {
            // show PIN dialog
            var win = new BMTECHRD.Pos.App.Windows.PinWindow();
            var ok = win.ShowDialog();
            if (ok == true)
            {
                var pin = win.EnteredPin ?? string.Empty;
                var res = await _api.AccessTableAsync(table.Id, _actorUserId, pin);
                // notify UI
                if (res != null && res.AccessGranted)
                {
                    MessageBox.Show("Access granted", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Access denied", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return;
        }

        if (access.AccessGranted)
        {
            MessageBox.Show("Access granted", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async Task UpdatePositionAsync(object? param)
    {
        if (param is not TableModel table) return;
        await _api.UpdateTablePositionAsync(table.Id, table.PosX, table.PosY);
    }
}
