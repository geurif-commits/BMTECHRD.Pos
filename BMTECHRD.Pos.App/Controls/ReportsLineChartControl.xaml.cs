using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using BMTECHRD.Pos.App.Models;

namespace BMTECHRD.Pos.App.Controls;

public partial class ReportsLineChartControl : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), 
        typeof(List<SalesDailyItemModel>), 
        typeof(ReportsLineChartControl),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public List<SalesDailyItemModel>? ItemsSource
    {
        get => (List<SalesDailyItemModel>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ReportsLineChartControl control)
        {
            control.DrawChart();
        }
    }

    public ReportsLineChartControl()
    {
        InitializeComponent();
        SizeChanged += (s, e) => DrawChart();
    }

    private void DrawChart()
    {
        ChartCanvas.Children.Clear();

        var items = ItemsSource;
        if (items == null || items.Count == 0)
            return;

        var width = ChartCanvas.ActualWidth;
        var height = ChartCanvas.ActualHeight;

        if (width < 100 || height < 100)
            return;

        // Márgenes
        const double marginLeft = 60;
        const double marginBottom = 40;
        const double marginTop = 20;
        const double marginRight = 20;

        var chartWidth = width - marginLeft - marginRight;
        var chartHeight = height - marginBottom - marginTop;

        // Encontrar min/max
        var maxValue = items.Max(x => x.Total);
        if (maxValue <= 0) maxValue = 100;

        // Dibujar ejes
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(150, 150, 150)), 1);

        // Eje Y
        var line1 = new Line
        {
            X1 = marginLeft,
            Y1 = marginTop,
            X2 = marginLeft,
            Y2 = height - marginBottom,
            Stroke = pen.Brush,
            StrokeThickness = 1
        };
        ChartCanvas.Children.Add(line1);

        // Eje X
        var line2 = new Line
        {
            X1 = marginLeft,
            Y1 = height - marginBottom,
            X2 = width - marginRight,
            Y2 = height - marginBottom,
            Stroke = pen.Brush,
            StrokeThickness = 1
        };
        ChartCanvas.Children.Add(line2);

        // Puntos del gráfico
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(30, 76, 175, 80))
        };

        for (int i = 0; i < items.Count; i++)
        {
            var x = marginLeft + (chartWidth / (items.Count - 1 > 0 ? items.Count - 1 : 1)) * i;
            var y = height - marginBottom - (chartHeight / (double)maxValue) * (double)items[i].Total;
            polyline.Points.Add(new Point(x, y));

            // Pequeño círculo en cada punto
            var ellipse = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80))
            };
            Canvas.SetLeft(ellipse, x - 3);
            Canvas.SetTop(ellipse, y - 3);
            ChartCanvas.Children.Add(ellipse);

            // Etiqueta de fecha (cada 2 puntos para evitar solapamiento)
            if (i % Math.Max(1, items.Count / 5) == 0)
            {
                var label = new TextBlock
                {
                    Text = items[i].DateLabel,
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    FontSize = 10
                };
                Canvas.SetLeft(label, x - 30);
                Canvas.SetTop(label, height - marginBottom + 5);
                ChartCanvas.Children.Add(label);
            }
        }

        ChartCanvas.Children.Add(polyline);

        // Etiqueta eje Y
        var yLabel = new TextBlock
        {
            Text = "$",
            Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            FontSize = 12,
            FontWeight = FontWeights.Bold
        };
        Canvas.SetLeft(yLabel, marginLeft - 20);
        Canvas.SetTop(yLabel, marginTop);
        ChartCanvas.Children.Add(yLabel);
    }
}
