using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BMTECHRD.Pos.App.Windows;

public partial class PinWindow : Window
{
    public string? EnteredPin { get; private set; }

    public PinWindow()
    {
        InitializeComponent();
        BtnOk.Click += BtnOk_Click;
        BtnCancel.Click += (s,e) => { DialogResult = false; };

        Box0.PreviewTextInput += DigitOnly;
        Box1.PreviewTextInput += DigitOnly;
        Box2.PreviewTextInput += DigitOnly;
        Box3.PreviewTextInput += DigitOnly;

        Box0.KeyUp += (s,e) => { if (Box0.Text.Length==1) Box1.Focus(); };
        Box1.KeyUp += (s,e) => { if (Box1.Text.Length==1) Box2.Focus(); };
        Box2.KeyUp += (s,e) => { if (Box2.Text.Length==1) Box3.Focus(); };
    }

    private void DigitOnly(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !char.IsDigit(e.Text, 0);
    }

    private void BtnOk_Click(object? sender, RoutedEventArgs e)
    {
        EnteredPin = (Box0.Text + Box1.Text + Box2.Text + Box3.Text);
        if (EnteredPin.Length != 4) { MessageBox.Show("Enter 4 digits", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        DialogResult = true;
    }
}
