using System.Windows;
using System.Windows.Controls;

namespace BMTECHRD.Pos.App.Helpers;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached(
        "BindPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty UpdatingPasswordProperty = DependencyProperty.RegisterAttached(
        "UpdatingPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

    public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);
    public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);

    public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);
    public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);

    private static bool GetUpdatingPassword(DependencyObject dp) => (bool)dp.GetValue(UpdatingPasswordProperty);
    private static void SetUpdatingPassword(DependencyObject dp, bool value) => dp.SetValue(UpdatingPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
        if (dp is not PasswordBox passwordBox) return;
        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

        if (!GetUpdatingPassword(passwordBox))
        {
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        }
        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
        if (dp is not PasswordBox passwordBox) return;

        var wasBound = (bool)e.OldValue;
        var needBound = (bool)e.NewValue;

        if (wasBound)
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        }

        if (needBound)
        {
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox) return;
        SetUpdatingPassword(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetUpdatingPassword(passwordBox, false);
    }
}
