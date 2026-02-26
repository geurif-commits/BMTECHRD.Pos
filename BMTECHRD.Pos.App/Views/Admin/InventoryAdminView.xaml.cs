using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels.Admin;

namespace BMTECHRD.Pos.App.Views.Admin;

public partial class InventoryAdminView : UserControl
{
    private InventoryAdminViewModel? _vm;
    public InventoryAdminView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, System.Guid businessId)
    {
        _vm = new InventoryAdminViewModel(api, businessId);
        DataContext = _vm;
    }
}
