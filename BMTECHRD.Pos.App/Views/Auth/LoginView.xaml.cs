using System.Windows;
using System.Windows.Controls;
using BMTECHRD.Pos.App.ViewModels.Auth;

namespace BMTECHRD.Pos.App.Views.Auth;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
        {
            vm.SetPassword(pb.Password);
        }
    }
}