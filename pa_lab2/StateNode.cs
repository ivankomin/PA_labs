public class StateNode
{
    public int[] Board { get; }
    public int Score { get; }

    public StateNode(int[] board, Func<int[], int> heuristicFunc)
    {
        Board = (int[])board.Clone();
        Score = heuristicFunc(board);
    }
    public StateNode(int[] board, int score)
    {
        Board = (int[])board.Clone();
        Score = score;
    }
}