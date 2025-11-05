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
        return this.F_Cost.CompareTo(other.F_Cost);
    }
}