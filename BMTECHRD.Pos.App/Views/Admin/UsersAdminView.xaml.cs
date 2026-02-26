using System;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels.Admin;

namespace BMTECHRD.Pos.App.Views.Admin;

public partial class UsersAdminView : UserControl
{
    private UsersAdminViewModel? _vm;
    public UsersAdminView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, Guid businessId, Guid actorUserId)
    {
        _vm = new UsersAdminViewModel(api, businessId, actorUserId);
        DataContext = _vm;
    }
}
