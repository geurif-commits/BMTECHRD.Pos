using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Views;

public partial class BarQueueView : UserControl
{
    private BarQueueViewModel? _vm;
    public BarQueueView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, SignalRClient signalR, System.Guid businessId)
    {
        _vm = new BarQueueViewModel(api, signalR, businessId);
        DataContext = _vm;

        BtnRefresh.Click += async (s, e) => await _vm.RefreshAsync();
        BtnPending.Click += (s, e) => _vm.Filter = "PENDING";
        BtnInProgress.Click += (s, e) => _vm.Filter = "IN_PROGRESS";
        BtnAll.Click += (s, e) => _vm.Filter = "ALL";

        // item double-click to Start/Done
        // item double-click to Start/Done
        ItemsListView.MouseDoubleClick += (s, e) =>
        {
            if (ItemsListView.SelectedItem is ProductionQueueItemModel item)
            {
                if (item.Status == "SENT") _vm.StartCommand.Execute(item);
                else if (item.Status == "IN_PROGRESS") _vm.DoneCommand.Execute(item);
            }
        };

        _ = _vm.RefreshAsync();
    }
}
