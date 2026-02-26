using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Controls;

public partial class DraggableTableControl : UserControl
{
    private bool _isDragging;
    private Point _startMousePos;
    private double _startX;
    private double _startY;

    public DraggableTableControl()
    {
        InitializeComponent();
        Loaded += DraggableTableControl_Loaded;
    }

    private void DraggableTableControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TableModel model)
        {
            // show badge if waiter name present
            Badge.Visibility = string.IsNullOrEmpty(model.WaiterName) ? Visibility.Collapsed : Visibility.Visible;

            // set color based on status
            if (string.Equals(model.Status, "OPEN", System.StringComparison.OrdinalIgnoreCase))
            {
                RootBorder.Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)); // red
            }
            else
            {
                RootBorder.Background = new SolidColorBrush(Color.FromRgb(56, 142, 60)); // green
            }
        }

        RootBorder.MouseLeftButtonDown += RootBorder_MouseLeftButtonDown;
        RootBorder.MouseMove += RootBorder_MouseMove;
        RootBorder.MouseLeftButtonUp += RootBorder_MouseLeftButtonUp;
        RootBorder.MouseLeftButtonUp += RootBorder_MouseClick;
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _startMousePos = e.GetPosition(null);
        if (DataContext is TableModel model)
        {
            _startX = model.PosX;
            _startY = model.PosY;
        }
        RootBorder.CaptureMouse();
    }

    private void RootBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var pos = e.GetPosition(this.Parent as UIElement);
        if (DataContext is TableModel model)
        {
            model.PosX = pos.X - (this.ActualWidth / 2);
            model.PosY = pos.Y - (this.ActualHeight / 2);
        }
    }

    private void RootBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        RootBorder.ReleaseMouseCapture();

        // invoke update command if available on DataContext
        if (DataContext is TableModel model)
        {
            // try to find command on parent DataContext (TablesMapViewModel)
            if (FindParent<FrameworkElement>(this) is FrameworkElement fe && fe.DataContext is BMTECHRD.Pos.App.ViewModels.TablesMapViewModel vm)
            {
                vm.UpdatePositionCommand.Execute(model);
            }
        }
    }

    private void RootBorder_MouseClick(object sender, MouseButtonEventArgs e)
    {
        // handle click (open or access)
        if (DataContext is TableModel model)
        {
            if (FindParent<FrameworkElement>(this) is FrameworkElement fe && fe.DataContext is BMTECHRD.Pos.App.ViewModels.TablesMapViewModel vm)
            {
                vm.OpenOrAccessTableCommand.Execute(model);
            }
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent == null) return null;
        if (parent is T t) return t;
        return FindParent<T>(parent);
    }
}
