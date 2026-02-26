using System;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels.Admin;

namespace BMTECHRD.Pos.App.Views.Admin;

public partial class ShiftsAdminView : UserControl
{
    public ShiftsAdminView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, Guid businessId, Guid actorUserId)
    {
        DataContext = new ShiftsAdminViewModel(api, businessId, actorUserId);
    }
}
