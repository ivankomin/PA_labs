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

public class RbfsNode : IComparable<RbfsNode>
{
    public int[] Board { get; }
    public int G_Cost { get; }
    public int H_Cost { get; }
    public int F_Cost { get; set; }

    public RbfsNode(int[] board, int g_cost, Func<int[], int> heuristicFunc)
    {
        Board = board;
        G_Cost = g_cost;
        H_Cost = heuristicFunc(board);
        F_Cost = G_Cost + H_Cost;
    }

    public int CompareTo(RbfsNode? other)
    {
        return F_Cost.CompareTo(other?.F_Cost);
    }

}
public class RbfsResult
{
    public RbfsNode? Node { get; }
    public int BackedUp_F_Cost { get; }

    public RbfsResult(RbfsNode? node, int backedUpCost)
    {
        Node = node;
        BackedUp_F_Cost = backedUpCost;
    }
}

public class SolveResult
{
    public StateNode? FinalNode { get; set; } 
    public long Iterations { get; set; }
    public long GeneratedNodes { get; set; }
    public int NodesInMemory { get; set; }

    public bool IsSuccess => FinalNode != null && FinalNode.Score == 0;
}