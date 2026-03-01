using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Controls;

public partial class DraggableTableControl : UserControl
{
    private bool _isDragging;
    private bool _moved;
    private Point _startMousePos;

    public DraggableTableControl()
    {
        InitializeComponent();
        Loaded += DraggableTableControl_Loaded;
    }

    private void DraggableTableControl_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyVisualState();

        RootBorder.MouseLeftButtonDown += RootBorder_MouseLeftButtonDown;
        RootBorder.MouseMove += RootBorder_MouseMove;
        RootBorder.MouseLeftButtonUp += RootBorder_MouseLeftButtonUp;
    }

    private void ApplyVisualState()
    {
        if (DataContext is not TableModel model) return;

        Badge.Visibility = string.IsNullOrWhiteSpace(model.WaiterName) ? Visibility.Collapsed : Visibility.Visible;

        if (string.Equals(model.Status, "OPEN", StringComparison.OrdinalIgnoreCase))
        {
            RootBorder.Background = new SolidColorBrush(Color.FromRgb(30, 58, 138));
            RootBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(96, 165, 250));
            return;
        }

        RootBorder.Background = new SolidColorBrush(Color.FromRgb(6, 78, 59));
        RootBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(52, 211, 153));
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _moved = false;
        _startMousePos = e.GetPosition(FindParent<Canvas>(this) ?? this.Parent as UIElement);
        RootBorder.CaptureMouse();
    }

    private void RootBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var canvas = FindParent<Canvas>(this);
        if (canvas == null || DataContext is not TableModel model) return;

        var pos = e.GetPosition(canvas);
        if (!_moved && (Math.Abs(pos.X - _startMousePos.X) > 4 || Math.Abs(pos.Y - _startMousePos.Y) > 4))
        {
            _moved = true;
        }

        if (_moved)
        {
            model.PosX = pos.X - (ActualWidth / 2);
            model.PosY = pos.Y - (ActualHeight / 2);
        }
    }

    private void RootBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;

        _isDragging = false;
        RootBorder.ReleaseMouseCapture();

        if (DataContext is not TableModel model) return;

        if (FindParent<FrameworkElement>(this)?.DataContext is not BMTECHRD.Pos.App.ViewModels.TablesMapViewModel vm) return;

        if (_moved)
        {
            vm.UpdatePositionCommand.Execute(model);
            return;
        }

        vm.OpenOrAccessTableCommand.Execute(model);
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent == null) return null;
        if (parent is T t) return t;
        return FindParent<T>(parent);
    }
}
