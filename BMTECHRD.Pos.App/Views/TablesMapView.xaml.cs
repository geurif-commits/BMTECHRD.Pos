using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;

namespace BMTECHRD.Pos.App.Views;

public partial class TablesMapView : UserControl
{
    public TablesMapView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, SignalRClient signalR, System.Guid businessId, System.Guid actorUserId)
    {
        DataContext = new TablesMapViewModel(api, businessId, actorUserId);
        if (DataContext is TablesMapViewModel vm)
        {
            _ = vm.LoadTablesAsync();

            // subscribe to realtime events to refresh tables
            signalR.OnTablesUpdated += async () => await vm.LoadTablesAsync();
            signalR.OnInventoryUpdated += async () => await vm.LoadTablesAsync();
        }
    }
}
