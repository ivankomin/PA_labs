using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace pa_lab6
{
    // --- МОДЕЛІ ДАНИХ ДЛЯ UI ---
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
            ProgressBar.Visibility = Visibility.Visible;
            Experiments.Clear();

            await Task.Run(() => RunOptimization());

            ProgressBar.Visibility = Visibility.Collapsed;
            BtnStart.Visibility = Visibility.Visible;
            MessageBox.Show("Оптимізацію завершено!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RunOptimization()
        {
            // 1. Створення графа
            var graph = new Graph(300, 2, 30);
            
            // Базові параметри
            var currentParams = new GeneticParams();

            // Список досліджень (параметр -> список значень для перевірки)
            var studies = new List<(string Name, List<double> Values, Action<GeneticParams, double> Setter)>
            {
                ("Кількість поколінь (Generations)", new List<double>{ 10, 50, 100, 200 }, (p, v) => p.Generations = (int)v),
                ("Розмір популяції (PopSize)", new List<double>{ 20, 50, 100, 150 }, (p, v) => p.PopSize = (int)v),
                ("Ймовірність схрещування (CrossRate)", new List<double>{ 0.5, 0.7, 0.9, 1.0 }, (p, v) => p.CrossRate = v),
                ("Ймовірність мутації (MutRate)", new List<double>{ 0.01, 0.05, 0.1, 0.2 }, (p, v) => p.MutRate = v)
            };

            // Цикл покоординатного спуску
            foreach (var study in studies)
            {
                var experiment = new ExperimentData { ParameterName = study.Name };
                
                // Додаємо в UI (через Dispatcher)
                Dispatcher.Invoke(() => Experiments.Add(experiment));

                double bestValForThisParam = -1;
                int bestResultForThisParam = int.MaxValue;

                foreach (var val in study.Values)
                {
                    // Клонуємо параметри і встановлюємо досліджуване значення
                    var testParams = currentParams.Clone();
                    study.Setter(testParams, val);

                    // Запускаємо ГА (можна кілька разів для середнього, тут 1 раз для швидкості)
                    var solver = new GeneticSolver(graph, testParams);
                    int result = solver.Run();

                    // Зберігаємо результат
                    Dispatcher.Invoke(() => 
                    {
                        experiment.DataPoints.Add(new DataPoint { XValue = val, YResult = result });
                        // Оновлюємо графік
                        RefreshCharts(); 
                    });

                    // Шукаємо кращий
                    if (result < bestResultForThisParam)
                    {
                        bestResultForThisParam = result;
                        bestValForThisParam = val;
                    }
                }

                // ФІКСУЄМО кращий параметр для наступних ітерацій
                study.Setter(currentParams, bestValForThisParam);
            }
        }

        // --- МЕТОД ДЛЯ МАЛЮВАННЯ ГРАФІКІВ (БЕЗ БІБЛІОТЕК) ---
        // Цей метод викликається, коли DataTemplate завантажується
        // --- ВИПРАВЛЕНИЙ БЛОК МАЛЮВАННЯ ---

        private void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            var canvas = sender as Canvas;
            if (canvas == null) return;

            // 1. Малюємо те, що вже є
            DrawChart(canvas);

            // 2. Підписуємося на зміни в даних, щоб перемальовувати графік при додаванні точок
            if (canvas.DataContext is ExperimentData data)
            {
                // Спочатку відписуємося, щоб не дублювати події, якщо Loaded спрацює двічі
                data.DataPoints.CollectionChanged -= (s, args) => DrawChart(canvas);
                data.DataPoints.CollectionChanged += (s, args) => DrawChart(canvas);
            }
        }

        // Цей метод можна видалити або залишити пустим, він більше не потрібен, 
        // оскільки ми використовуємо CollectionChanged
        private void RefreshCharts() 
        { 
            // Логіка тепер автоматична через подію
        }

        private void DrawChart(Canvas canvas)
        {
            if (canvas == null || canvas.DataContext is not ExperimentData data) return;
            
            // Якщо точок немає або вона одна - малювати графік ще рано/неможливо
            if (data.DataPoints.Count < 1) 
            {
                canvas.Children.Clear();
                return;
            }

            canvas.Children.Clear();
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            
            // Якщо Canvas ще не має розміру (наприклад, при першому завантаженні)
            if (w == 0 || h == 0) return;

            // Відступи
            double margin = 30;

            // Визначаємо межі для осей X та Y
            double xMax = data.DataPoints.Max(p => p.XValue);
            double xMin = data.DataPoints.Min(p => p.XValue);
            
            // Для Y беремо 0 як мінімум, щоб графік був наочнішим (або min значення, якщо вони великі)
            double yMax = data.DataPoints.Max(p => p.YResult);
            double yMin = data.DataPoints.Min(p => p.YResult);

            // Додаємо трохи "повітря" зверху і знизу графіка
            double rangeY = yMax - yMin;
            if (rangeY == 0) rangeY = 10; // Захист, якщо всі значення однакові
            
            yMax += rangeY * 0.1; 
            yMin -= rangeY * 0.1;
            if (yMin < 0) yMin = 0; // Не йдемо в мінус, якщо це кількість вершин

            double rangeX = xMax - xMin;
            if (rangeX == 0) rangeX = 1;

            // Малюємо осі
            var axisX = new Line { X1 = margin, Y1 = h - margin, X2 = w, Y2 = h - margin, Stroke = Brushes.Black, StrokeThickness = 1 };
            var axisY = new Line { X1 = margin, Y1 = 0, X2 = margin, Y2 = h - margin, Stroke = Brushes.Black, StrokeThickness = 1 };
            canvas.Children.Add(axisX);
            canvas.Children.Add(axisY);

            // Малюємо лінію з'єднання точок
            Polyline polyline = new Polyline { Stroke = Brushes.Blue, StrokeThickness = 2 };
            
            foreach (var p in data.DataPoints)
            {
                // Перетворення координат
                double x = margin + (p.XValue - xMin) / rangeX * (w - margin - 20);
                double y = (h - margin) - (p.YResult - yMin) / (yMax - yMin) * (h - margin - 20);

                polyline.Points.Add(new Point(x, y));

                // Малюємо точку
                Ellipse dot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Red };
                Canvas.SetLeft(dot, x - 3);
                Canvas.SetTop(dot, y - 3);
                
                // Додаємо тултіп (підказку) при наведенні миші
                dot.ToolTip = $"Param: {p.XValue}\nCover: {p.YResult}";
                
                canvas.Children.Add(dot);
                
                // Текстовий підпис значення над точкою
                TextBlock lbl = new TextBlock { Text = p.YResult.ToString("0"), FontSize = 10 };
                Canvas.SetLeft(lbl, x - 5);
                Canvas.SetTop(lbl, y - 18);
                canvas.Children.Add(lbl);
                
                // Текстовий підпис параметра знизу (вісь X)
                TextBlock lblX = new TextBlock { Text = p.XValue.ToString("0.##"), FontSize = 9, Foreground = Brushes.Gray };
                Canvas.SetLeft(lblX, x - 5);
                Canvas.SetTop(lblX, h - margin + 2);
                canvas.Children.Add(lblX);
            }
            canvas.Children.Add(polyline);

            // Підписка на зміну розміру вікна, щоб графік розтягувався
            canvas.SizeChanged -= Canvas_SizeChanged;
            canvas.SizeChanged += Canvas_SizeChanged;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart(sender as Canvas);
        }
}
}