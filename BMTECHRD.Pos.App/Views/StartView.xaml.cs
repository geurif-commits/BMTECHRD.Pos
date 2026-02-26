using System.Windows.Controls;
using BMTECHRD.Pos.App.Services;
using BMTECHRD.Pos.App.ViewModels;
using BMTECHRD.Pos.App.Models;
using System.Windows;

namespace BMTECHRD.Pos.App.Views;

public partial class StartView : UserControl
{
    private StartViewModel? _vm;
    public event System.Action<SessionModel>? OnLoginSuccess;

    public StartView()
    {
        InitializeComponent();
    }

    public void Initialize(ApiClient api, AuthSessionService session)
    {
        _vm = new StartViewModel(api, session);
        DataContext = _vm;
        _vm.OnLoginSuccess += s => OnLoginSuccess?.Invoke(s);

        BtnLogin.Click += (s, e) =>
        {
            if (_vm == null) return;
            _vm.Username = TxtUsername.Text;
            _vm.Password = TxtPassword.Password;
            _vm.LoginCommand.Execute(null);
        };

        CbBusinesses.SelectionChanged += (s,e) =>
        {
            if (CbBusinesses.SelectedItem is BusinessPublicModel b)
            {
                if (!string.IsNullOrEmpty(b.LogoPath))
                {
                    ImgLogo.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(new System.Uri("https://localhost:5001"), b.LogoPath));
                    ImgLogo.Visibility = Visibility.Visible;
                }
                else ImgLogo.Visibility = Visibility.Collapsed;
            }
        };

        // load businesses
        _vm.LoadBusinessesCommand.Execute(null);
    }
}
