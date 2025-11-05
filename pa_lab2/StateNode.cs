public class StateNode
{
    public int[] Board { get; }
    public int Score { get; } // Це H_Cost

    /// <summary>
    /// Конструктор 1: Для BEAM (і ініціалізації)
    /// Він приймає ФУНКЦІЮ і сам обраховує Score.
    /// </summary>
    public StateNode(int[] board, Func<int[], int> heuristicFunc)
    {
        Board = (int[])board.Clone();
        Score = heuristicFunc(board);
    }

    /// <summary>
    /// Конструктор 2: Для RBFSSolver (і результатів)
    /// Він приймає ВЖЕ ОБРАХОВАНИЙ Score.
    /// </summary>
    public StateNode(int[] board, int score)
    {
        Board = (int[])board.Clone();
        Score = score; // Просто зберігає score
    }
}