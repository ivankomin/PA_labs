namespace pa_lab2;
public class StateNode
{
    public int[] Board { get; set; }
    public int Score { get; }
    public StateNode(int[] board, Func<int[], int> heuristicFunc)
    {
        Board = (int[])board.Clone();
        Score = heuristicFunc(Board);
    }
}