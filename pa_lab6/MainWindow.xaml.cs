using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace pa_lab6;
public partial class MainWindow : Window
{
    public ObservableCollection<ExperimentData> Experiments { get; set; } = new ObservableCollection<ExperimentData>();

    public MainWindow()
    {
        InitializeComponent();
        ResultsPanel.ItemsSource = Experiments;
    }

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        BtnStart.Visibility = Visibility.Collapsed;
        Experiments.Clear();
        await Task.Run(() => RunOptimization());
        BtnStart.Visibility = Visibility.Visible;
        MessageBox.Show("Оптимізацію завершено!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RunOptimization()
    {
    var graph = new Graph(300, 2, 30);

    Dispatcher.Invoke(() => SummaryCard.Visibility = Visibility.Collapsed);

    var initialParams = new GeneticParams(); 
    initialParams.Generations = 0; 

    var initialSolver = new GeneticSolver(graph, initialParams);
    int startScore = initialSolver.Run();

    int absoluteBestScore = startScore;

    var currentParams = new GeneticParams();

    var studies = new List<(string Name, List<double> Values, Action<GeneticParams, double> Setter)>
    {
        ("Кількість поколінь", new List<double>{ 10, 50, 100, 200 }, (p, v) => p.Generations = (int)v),
        ("Розмір популяції", new List<double>{ 20, 50, 100, 150 }, (p, v) => p.PopSize = (int)v),
        ("Ймовірність схрещування", new List<double>{ 0.5, 0.7, 0.9, 1.0 }, (p, v) => p.CrossRate = v),
        ("Ймовірність мутації", new List<double>{ 0.01, 0.05, 0.1, 0.2 }, (p, v) => p.MutRate = v)
    };

    foreach (var study in studies)
    {
        var experiment = new ExperimentData { ParameterName = study.Name };
        Dispatcher.Invoke(() => Experiments.Add(experiment));

        double bestValForParam = -1;
        int bestResForParam = int.MaxValue;

        foreach (var val in study.Values)
        {
            var testParams = currentParams.Clone();
            study.Setter(testParams, val);

            int runsCount = 5; 
            double sumResult = 0;
            int bestRunResult = int.MaxValue;

            for (int i = 0; i < runsCount; i++)
            {
                var solver = new GeneticSolver(graph, testParams);
                int runRes = solver.Run();
                
                sumResult += runRes;

                if (runRes < bestRunResult) bestRunResult = runRes;
                if (runRes < absoluteBestScore) absoluteBestScore = runRes;
            }
            double averageResult = sumResult / runsCount; 

            Dispatcher.Invoke(() => 
            {
                experiment.DataPoints.Add(new DataPoint { XValue = val, YResult = averageResult });
            });

            if (averageResult < bestResForParam)
            {
                bestResForParam = (int)averageResult;
                bestValForParam = val;
            }
        }
        study.Setter(currentParams, bestValForParam);
    }

    Dispatcher.Invoke(() =>
    {
        TxtBefore.Text = startScore.ToString();
        TxtAfter.Text = absoluteBestScore.ToString();

        double maxVal = Math.Max(startScore, absoluteBestScore);
        if (maxVal == 0) maxVal = 1;

        BarBefore.Maximum = maxVal;
        BarBefore.Value = startScore;
        
        BarAfter.Maximum = maxVal;
        BarAfter.Value = absoluteBestScore;

        double diff = startScore - absoluteBestScore;
        double improvement = 0;
        if (startScore > 0)
            improvement = (diff / startScore) * 100.0;
        
        if (diff > 0)
        {
            TxtImprovement.Text = $"Зменшено кількість вершин на {diff} ({improvement:F1}%)";
            TxtImprovement.Foreground = Brushes.Green;
        }
        else
        {
            TxtImprovement.Text = "Покращення не знайдено (оптимально з початку)";
            TxtImprovement.Foreground = Brushes.Gray;
        }
        
        SummaryCard.Visibility = Visibility.Visible;
    });
    }

    private void Chart_Loaded(object sender, RoutedEventArgs e)
    {
        var canvas = sender as Canvas;
        if (canvas == null) return;

        DrawChart(canvas);

        if (canvas.DataContext is ExperimentData data)
        {
            data.DataPoints.CollectionChanged -= (s, args) => DrawChart(canvas);
            data.DataPoints.CollectionChanged += (s, args) => DrawChart(canvas);
        }
    }
    private void RefreshCharts() 
    { 

    }

    private void DrawChart(Canvas canvas)
    {
        if (canvas == null || canvas.DataContext is not ExperimentData data) return;
        if (data.DataPoints.Count < 1) 
        {
            canvas.Children.Clear();
            return;
        }

        canvas.Children.Clear();
        double w = canvas.ActualWidth;
        double h = canvas.ActualHeight;
        if (w == 0 || h == 0) return;
        double margin = 30;
        double xMax = data.DataPoints.Max(p => p.XValue);
        double xMin = data.DataPoints.Min(p => p.XValue);
        double yMax = data.DataPoints.Max(p => p.YResult);
        double yMin = data.DataPoints.Min(p => p.YResult);

        double rangeY = yMax - yMin;
        if (rangeY == 0) rangeY = 10; 
        
        yMax += rangeY * 0.1; 
        yMin -= rangeY * 0.1;
        if (yMin < 0) yMin = 0; 

        double rangeX = xMax - xMin;
        if (rangeX == 0) rangeX = 1;

        var axisX = new Line { X1 = margin, Y1 = h - margin, X2 = w, Y2 = h - margin, Stroke = Brushes.Black, StrokeThickness = 1 };
        var axisY = new Line { X1 = margin, Y1 = 0, X2 = margin, Y2 = h - margin, Stroke = Brushes.Black, StrokeThickness = 1 };
        canvas.Children.Add(axisX);
        canvas.Children.Add(axisY);

        Polyline polyline = new Polyline { Stroke = Brushes.Blue, StrokeThickness = 2 };
        
        foreach (var p in data.DataPoints)
        {
            double x = margin + (p.XValue - xMin) / rangeX * (w - margin - 20);
            double y = (h - margin) - (p.YResult - yMin) / (yMax - yMin) * (h - margin - 20);

            polyline.Points.Add(new Point(x, y));

            Ellipse dot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Red };
            Canvas.SetLeft(dot, x - 3);
            Canvas.SetTop(dot, y - 3);

            dot.ToolTip = $"Param: {p.XValue}\nCover: {p.YResult}";
            
            canvas.Children.Add(dot);

            TextBlock lbl = new TextBlock { Text = p.YResult.ToString("0"), FontSize = 10 };
            Canvas.SetLeft(lbl, x - 5);
            Canvas.SetTop(lbl, y - 18);
            canvas.Children.Add(lbl);

            TextBlock lblX = new TextBlock { Text = p.XValue.ToString("0.##"), FontSize = 9, Foreground = Brushes.Gray };
            Canvas.SetLeft(lblX, x - 5);
            Canvas.SetTop(lblX, h - margin + 2);
            canvas.Children.Add(lblX);
        }
        canvas.Children.Add(polyline);
        canvas.SizeChanged -= Canvas_SizeChanged;
        canvas.SizeChanged += Canvas_SizeChanged;
    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawChart(sender as Canvas);
    }
}