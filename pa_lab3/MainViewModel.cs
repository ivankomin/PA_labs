using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace pa_lab3
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
    }

    public class CellVM : INotifyPropertyChanged
    {
        private bool _isActive;
        private Player? _eatenBy; 
        private bool _isMoveOrigin;

        public int Row { get; }
        public int Col { get; }
        public bool IsPoison => Row == 0 && Col == 0;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; UpdateVisuals(); }
        }
        public Player? EatenBy
        {
            get => _eatenBy;
            set { _eatenBy = value; UpdateVisuals(); }
        }
        public bool IsMoveOrigin
        {
            get => _isMoveOrigin;
            set { _isMoveOrigin = value; UpdateVisuals(); }
        }
        public Brush BackgroundColor { get; private set; }
        
        public string Symbol { get; private set; }
        
        public Brush ForegroundColor { get; private set; }

        public ICommand ClickCommand { get; }

        public CellVM(int r, int c, ICommand clickParams)
        {
            Row = r; Col = c;
            ClickCommand = clickParams;
            Reset();
        }

        public void Reset()
        {
            _isActive = true;
            _eatenBy = null;
            _isMoveOrigin = false;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (IsActive)
            {
                BackgroundColor = IsPoison ? Brushes.ForestGreen : Brushes.SaddleBrown;
            }
            else
            {
                BackgroundColor = EatenBy == Player.User 
                    ? new SolidColorBrush(Color.FromRgb(240, 128, 128))  
                    : new SolidColorBrush(Color.FromRgb(135, 206, 235)); 
            }
            if (IsPoison)
            {
                Symbol = "☠";
                ForegroundColor = Brushes.White;
            }
            else if (!IsActive && IsMoveOrigin)
            {
                Symbol = EatenBy == Player.User ? "X" : "O";
                ForegroundColor = EatenBy == Player.User ? Brushes.DarkRed : Brushes.DarkBlue;
            }
            else
            {
                Symbol = "";
                ForegroundColor = Brushes.Black;
            }

            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(Symbol));
            OnPropertyChanged(nameof(ForegroundColor));
            OnPropertyChanged(nameof(IsActive));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private GameState _gameState;
        private readonly ChompAI _ai;
        private bool _isUserTurn;
        private string _statusMessage;
        private Difficulty _selectedDifficulty;
        
        public ObservableCollection<CellVM> Cells { get; set; } = new();
        
        public int Rows { get; set; } = 6;
        public int Cols { get; set; } = 8;

        public Difficulty SelectedDifficulty
        {
            get => _selectedDifficulty;
            set { _selectedDifficulty = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand NewGameCommand { get; }
        public ICommand CellClickCommand { get; }

        public  MainViewModel()
        {
            _ai = new ChompAI();
            CellClickCommand = new RelayCommand(OnCellClicked);
            NewGameCommand = new RelayCommand(_ => StartNewGame());
            SelectedDifficulty = Difficulty.Medium;
            
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    Cells.Add(new CellVM(r, c, CellClickCommand));

            StartNewGame();
        }

        private void StartNewGame()
        {
            _gameState = new GameState(Rows, Cols);
            _isUserTurn = true;
            StatusMessage = "Ваш хід! (Уникайте отрути)";
            
            foreach (var cell in Cells) cell.Reset();
        }

        private async void OnCellClicked(object parameter)
        {
            if (!_isUserTurn || parameter is not CellVM cell || !cell.IsActive) return;

            PerformMove(new Move(cell.Row, cell.Col), Player.User);
            if (CheckGameOver(Player.User)) return;

            _isUserTurn = false;
            StatusMessage = "Комп'ютер думає...";
            
            await Task.Delay(500);
            var aiMove = await Task.Run(() => _ai.GetBestMove(_gameState, SelectedDifficulty));
            
            PerformMove(aiMove, Player.Computer);
            
            if (!CheckGameOver(Player.Computer))
            {
                _isUserTurn = true;
                StatusMessage = "Ваш хід!";
            }
        }

        private void PerformMove(Move move, Player player)
        {
            _gameState = _gameState.MakeMove(move);

            foreach(var cell in Cells)
            {
                if (cell.IsActive && !_gameState.IsCellActive(cell.Row, cell.Col))
                {
                    cell.IsActive = false;     
                    cell.EatenBy = player;

                    if (cell.Row == move.Row && cell.Col == move.Col)
                    {
                        cell.IsMoveOrigin = true;
                    }
                }
            }
        }

        private bool CheckGameOver(Player lastPlayer)
        {
            if (!_gameState.IsCellActive(0, 0))
            {
                string winner = lastPlayer == Player.User ? "Комп'ютер" : "Гравець";
                StatusMessage = $"Гру закінчено! {winner} переміг!";
                _isUserTurn = false;
                MessageBox.Show($"{winner} переміг!", "Кінець гри");
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}