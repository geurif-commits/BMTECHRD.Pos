using System;
using System.Collections.Generic;
using System.Windows;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;

namespace BMTECHRD.Pos.App.Views;

public partial class TableOrderWindow : Window
{
    public TableOrderWindow(
        ApiClient api,
        Guid businessId,
        Guid tableId,
        Guid actorUserId,
        int tableNumber,
        IReadOnlyCollection<TableOrderLineModel> initialDraft,
        Action<IReadOnlyCollection<TableOrderLineModel>> persistDraft)
    {
        InitializeComponent();
        var vm = new TableOrderWindowViewModel(api, businessId, tableId, actorUserId, tableNumber, initialDraft, persistDraft);
        vm.OrderSent += Close;
        DataContext = vm;
        Loaded += async (_, _) => await vm.InitializeAsync();
    }
}
