using System;
using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels.Admin;

namespace BMTECHRD.Pos.App.Views.Admin;

public partial class BusinessSettingsView : UserControl
{
    private BusinessSettingsViewModel? _viewModel;

    public BusinessSettingsView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, Guid businessId)
    {
        _viewModel = new BusinessSettingsViewModel(api, businessId);
        DataContext = _viewModel;
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        _viewModel?.SaveCommand.Execute(null);
    }

    private void OnReloadClick(object sender, RoutedEventArgs e)
    {
        _viewModel?.LoadCommand.Execute(null);
    }
}
