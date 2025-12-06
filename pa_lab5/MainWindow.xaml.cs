using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace pa_lab5; 
public partial class MainWindow : Window
{
    const int NUM_VERTICES = 150;
    const int MAX_DEGREE = 30;
    const int MIN_DEGREE = 1;
    const int NUM_BEES = 25;
    const int NUM_SCOUTS = 3;
    const int MAX_ITERATIONS = 1000;
    const int REPORT_STEP = 20;

    private List<HashSet<int>> _adjacencyList = new();
    private List<Point> _nodePositions = new();
    private List<int> _bestColoring = new();
    //for chart
    private List<(int Iteration, int Colors)> _history = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        BtnRun.IsEnabled = false;
        BtnShowChart.IsEnabled = false;
        
        TxtStatus.Text = "Генерація графу...";
        GraphCanvas.Children.Clear();

        await Task.Run(GenerateGraphCircular);
        DrawGraphNodesOnly(); 

        TxtStatus.Text = "Запуск алгоритму ABC...";

        await Task.Run(RunABCAlgorithm);

        TxtStatus.Text = "Готово.";
        TxtResult.Text = $"Мін. кольорів: {_history.Last().Colors}";
        
        DrawGraphColors();

        // ... (кінець методу BtnRun_Click, перед BtnRun.IsEnabled = true;)

// --- ВАРІАНТ: ЗБЕРЕЖЕННЯ У ФАЙЛ (Щоб точно побачити) ---
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("--- ДАНІ ДЛЯ ТАБЛИЦІ 3.1 ---");
        sb.AppendLine("Ітерація \t К-сть кольорів");

        foreach (var item in _history)
        {
            sb.AppendLine($"{item.Iteration} \t\t {item.Colors}");
        }

// Шлях до файлу на Робочому столі
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = System.IO.Path.Combine(desktopPath, "results_table.txt");

// Записуємо
        System.IO.File.WriteAllText(filePath, sb.ToString());

// Повідомляємо, що все готово
        MessageBox.Show($"Дані збережено у файл:\n{filePath}", "Готово!");

        BtnRun.IsEnabled = true;
        BtnShowChart.IsEnabled = true; 
        
        BtnShowChart_Click(null, null);
    }

    private void BtnShowChart_Click(object sender, RoutedEventArgs e)
    {
        if (_history == null || _history.Count == 0) return;
        ChartWindow chartWin = new ChartWindow(_history);
        chartWin.Owner = this; 
        chartWin.Show();
    }

    private void GenerateGraphCircular()
    {
        Random rnd = new Random();
        _adjacencyList = new List<HashSet<int>>();
        _nodePositions = new List<Point>();

        for (int i = 0; i < NUM_VERTICES; i++) _adjacencyList.Add(new HashSet<int>());

        for (int i = 0; i < NUM_VERTICES; i++)
        {
            while (_adjacencyList[i].Count < MIN_DEGREE)
            {
                int target = rnd.Next(NUM_VERTICES);
                if (target != i && !_adjacencyList[i].Contains(target) && _adjacencyList[target].Count < MAX_DEGREE)
                {
                    _adjacencyList[i].Add(target);
                    _adjacencyList[target].Add(i);
                }
            }
        }
        for (int i = 0; i < NUM_VERTICES; i++)
        {
            int targetDegree = rnd.Next(MIN_DEGREE, MAX_DEGREE + 1);
            int attempts = 0;
            while (_adjacencyList[i].Count < targetDegree && attempts < 50)
            {
                int target = rnd.Next(NUM_VERTICES);
                if (target != i && !_adjacencyList[i].Contains(target) && _adjacencyList[target].Count < MAX_DEGREE)
                {
                    _adjacencyList[i].Add(target);
                    _adjacencyList[target].Add(i);
                }
                attempts++;
            }
        }

        double centerX = 0.5;
        double centerY = 0.5;
        double radius = 0.45; 

        for (int i = 0; i < NUM_VERTICES; i++)
        {
            double angle = 2.0 * Math.PI * i / NUM_VERTICES;
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY + radius * Math.Sin(angle);
            _nodePositions.Add(new Point(x, y));
        }
    }

    private int GreedyColoring(List<int> order, out int[] nodeColors)
    {
        nodeColors = new int[NUM_VERTICES];
        for(int i=0; i<NUM_VERTICES; i++) nodeColors[i] = -1;

        int maxColorUsed = 0;
        foreach (var node in order)
        {
            HashSet<int> neighborColors = new();
            foreach (var neighbor in _adjacencyList[node])
            {
                if (nodeColors[neighbor] != -1)
                    neighborColors.Add(nodeColors[neighbor]);
            }
            int color = 0;
            while (neighborColors.Contains(color)) color++;
            nodeColors[node] = color;
            if (color + 1 > maxColorUsed) maxColorUsed = color + 1;
        }
        return maxColorUsed;
    }

    private void RunABCAlgorithm()
    {
        Random rnd = new Random();
        int employedCount = (NUM_BEES - NUM_SCOUTS) / 2;
        int onlookerCount = NUM_BEES - NUM_SCOUTS - employedCount;

        List<List<int>> population = new();
        List<int> fitness = new();

        // Init
        for (int i = 0; i < employedCount; i++)
        {
            var p = Enumerable.Range(0, NUM_VERTICES).OrderBy(x => rnd.Next()).ToList();
            population.Add(p);
            fitness.Add(GreedyColoring(p, out _));
        }

        _history = new List<(int, int)>();
        int globalBestFitness = int.MaxValue;
        List<int> globalBestOrder = new();

        for (int iter = 0; iter <= MAX_ITERATIONS; iter++)
        {
            // Employed
            for (int i = 0; i < employedCount; i++)
            {
                var newSol = Mutate(population[i], rnd);
                int newFit = GreedyColoring(newSol, out _);
                if (newFit <= fitness[i]) { population[i] = newSol; fitness[i] = newFit; }
            }

            // Onlooker
            double maxF = fitness.Max();
            var weights = fitness.Select(f => maxF - f + 1.0).ToList();
            double totalWeight = weights.Sum();

            for (int i = 0; i < onlookerCount; i++)
            {
                double r = rnd.NextDouble() * totalWeight;
                double sum = 0;
                int selectedIdx = 0;
                for (int j = 0; j < weights.Count; j++) { sum += weights[j]; if (sum >= r) { selectedIdx = j; break; } }

                var newSol = Mutate(population[selectedIdx], rnd);
                int newFit = GreedyColoring(newSol, out _);
                if (newFit <= fitness[selectedIdx]) { population[selectedIdx] = newSol; fitness[selectedIdx] = newFit; }
            }

            // Scout
            var sortedIndices = fitness.Select((val, idx) => new { Val = val, Idx = idx })
                                       .OrderByDescending(x => x.Val).Take(NUM_SCOUTS).ToList();
            foreach (var item in sortedIndices)
            {
                population[item.Idx] = Enumerable.Range(0, NUM_VERTICES).OrderBy(x => rnd.Next()).ToList();
                fitness[item.Idx] = GreedyColoring(population[item.Idx], out _);
            }

            // Update Best
            int currentMin = fitness.Min();
            if (currentMin < globalBestFitness)
            {
                globalBestFitness = currentMin;
                int bestIdx = fitness.IndexOf(currentMin);
                globalBestOrder = new List<int>(population[bestIdx]);
            }

            if (iter % REPORT_STEP == 0 || iter == MAX_ITERATIONS)
            {
                _history.Add((iter, globalBestFitness));
                
                // Update UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TxtStatus.Text = $"Ітерація {iter}/{MAX_ITERATIONS}. Мін. кольорів: {globalBestFitness}";
                    
                    var tempOrder = new List<int>(globalBestOrder);
                    GreedyColoring(tempOrder, out int[] tempColors);
                    _bestColoring = tempColors.ToList();
                    DrawGraphColors();
                });
            }
        }
    }

    private List<int> Mutate(List<int> solution, Random rnd)
    {
        var newSol = new List<int>(solution);
        int idx1 = rnd.Next(NUM_VERTICES);
        int idx2 = rnd.Next(NUM_VERTICES);
        (newSol[idx1], newSol[idx2]) = (newSol[idx2], newSol[idx1]);
        return newSol;
    }

    private Brush GetBrush(int colorIndex)
    {
        if (colorIndex == -1) return Brushes.Gray;
        double hue = colorIndex * 137.508; 
        return new SolidColorBrush(HslToRgb(hue % 360, 0.7, 0.5));
    }
    
    private Color HslToRgb(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;
        double r = 0, g = 0, b = 0;

        if (h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else { r = c; b = x; }
        return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }

    private void DrawGraphNodesOnly()
    {
        double w = GraphCanvas.ActualWidth;
        double h = GraphCanvas.ActualHeight;

        foreach (var u in Enumerable.Range(0, NUM_VERTICES))
        {
            foreach (var v in _adjacencyList[u])
            {
                if (u < v)
                {
                    Line line = new Line
                    {
                        X1 = _nodePositions[u].X * w,
                        Y1 = _nodePositions[u].Y * h,
                        X2 = _nodePositions[v].X * w,
                        Y2 = _nodePositions[v].Y * h,
                        Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)), 
                        StrokeThickness = 0.5
                    };
                    GraphCanvas.Children.Add(line);
                }
            }
        }

        for (int i = 0; i < NUM_VERTICES; i++)
        {
            Ellipse el = new Ellipse
            {
                Width = 10, Height = 10, 
                Fill = Brushes.LightGray,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
            Canvas.SetLeft(el, _nodePositions[i].X * w - 5);
            Canvas.SetTop(el, _nodePositions[i].Y * h - 5);
            GraphCanvas.Children.Add(el);
        }
    }

    private void DrawGraphColors()
    {
        int nodeIndex = 0;
        foreach (var child in GraphCanvas.Children)
        {
            if (child is Ellipse el)
            {
                int colorId = _bestColoring.Count > nodeIndex ? _bestColoring[nodeIndex] : -1;
                el.Fill = GetBrush(colorId);
                nodeIndex++;
            }
        }
    }
}