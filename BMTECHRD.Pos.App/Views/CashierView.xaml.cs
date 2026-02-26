using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;

namespace BMTECHRD.Pos.App.Views;

public partial class CashierView : UserControl
{
    private CashierViewModel? _vm;
    public CashierView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, SignalRClient signalR, System.Guid businessId, System.Guid actorUserId)
    {
        _vm = new CashierViewModel(api, signalR, businessId, actorUserId);
        DataContext = _vm;
        _ = _vm.RefreshAsync();
    }
}
