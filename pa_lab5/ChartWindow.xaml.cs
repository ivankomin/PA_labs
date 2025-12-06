using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace pa_lab5;

public partial class ChartWindow : Window
{
    private List<(int Iteration, int Colors)> _data;

    public ChartWindow(List<(int Iteration, int Colors)> data)
    {
        InitializeComponent();
        _data = data;
        Loaded += (s, e) => DrawChart();
    }

    private void PlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawChart();
    }

    private void DrawChart()
    {
        if (_data == null || _data.Count == 0 || PlotCanvas.ActualWidth == 0) return;

        PlotCanvas.Children.Clear();
        AxisX.Children.Clear();
        AxisY.Children.Clear();

        double w = PlotCanvas.ActualWidth;
        double h = PlotCanvas.ActualHeight;

        double minIter = 0;
        double maxIter = _data.Max(d => d.Iteration);
        
        double minColor = _data.Min(d => d.Colors);
        double maxColor = _data.Max(d => d.Colors);

        minColor = Math.Max(0, minColor - 1); 
        maxColor = maxColor + 1;

        double rangeX = maxIter - minIter;
        double rangeY = maxColor - minColor;

        int stepY = rangeY > 20 ? (int)(rangeY / 10) : 1;
        if (stepY < 1) stepY = 1;

        for (double yVal = Math.Floor(minColor); yVal <= maxColor; yVal += stepY)
        {
            double normalizedY = (yVal - minColor) / rangeY;
            double screenY = h - (normalizedY * h);
            Line gridLine = new Line
            {
                X1 = 0, Y1 = screenY,
                X2 = w, Y2 = screenY,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            PlotCanvas.Children.Add(gridLine);

            TextBlock label = new TextBlock
            {
                Text = yVal.ToString("0"),
                FontSize = 12,
                Foreground = Brushes.Black
            };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, AxisY.ActualWidth - label.DesiredSize.Width);
            Canvas.SetTop(label, screenY - label.DesiredSize.Height / 2);
            AxisY.Children.Add(label);
        }

        double stepX = 100;
        if (maxIter > 2000) stepX = 500;

        for (double xVal = 0; xVal <= maxIter; xVal += stepX)
        {
            double normalizedX = (xVal - minIter) / rangeX;
            double screenX = normalizedX * w;
            Line gridLine = new Line
            {
                X1 = screenX, Y1 = 0,
                X2 = screenX, Y2 = h,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            PlotCanvas.Children.Add(gridLine);

            TextBlock label = new TextBlock
            {
                Text = xVal.ToString(),
                FontSize = 12,
                Foreground = Brushes.Black
            };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, screenX - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, 5);
            AxisX.Children.Add(label);
        }

        Polyline polyline = new Polyline
        {
            Stroke = Brushes.Blue,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round
        };

        foreach (var p in _data)
        {
            double x = (p.Iteration - minIter) / rangeX * w;
            double y = h - ((p.Colors - minColor) / rangeY * h);
            polyline.Points.Add(new Point(x, y));
        }

        PlotCanvas.Children.Add(polyline);
    }
}