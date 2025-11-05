namespace pa_lab2;
public class RBFSSolver
{
    private Func<int[], int> _heuristicFunc;
    private int _generatedNodes;
    private int _maxNodesInMemory;
    private List<RbfsNode> GetSuccessors(RbfsNode node)
    {
        var successors = new List<RbfsNode>();
        int boardSize = node.Board.Length;

        for (int col = 0; col < boardSize; col++)
        {
            int currentRow = node.Board[col];
            for (int row = 0; row < boardSize; row++)
            {
                if (row == currentRow) continue;

                int[] newBoard = (int[])node.Board.Clone();
                newBoard[col] = row;
                successors.Add(new RbfsNode(newBoard, node.G_Cost + 1, _heuristicFunc));
            }
        }
        _generatedNodes += successors.Count;
        return successors;
    }

    public SolveResult Solve(int[] initialState, Func<int[], int> heuristicFunc, int maxIterations)
    {
        _heuristicFunc = heuristicFunc;
        _generatedNodes = 1; 
        _maxNodesInMemory = 0;

        var rootNode = new RbfsNode(initialState, 0, _heuristicFunc);
        var result = RecursiveSearch(rootNode, int.MaxValue, 0, maxIterations);

        return new SolveResult
        {
            FinalNode = (result.Node != null) ? new StateNode(result.Node.Board, result.Node.H_Cost) : null,
            Iterations = _generatedNodes,
            GeneratedNodes = _generatedNodes,
            NodesInMemory = _maxNodesInMemory
        };
    }

    private RbfsResult RecursiveSearch(RbfsNode node, int f_limit, int depth, int maxIterations)
    {
        if (depth > _maxNodesInMemory) _maxNodesInMemory = depth;
        if (_generatedNodes > maxIterations)
        {
            return new RbfsResult(null, int.MaxValue);
        }

        if (node.H_Cost == 0)
        {
            return new RbfsResult(node, node.F_Cost);
        }

        var successors = GetSuccessors(node);

        if (successors.Count == 0)
        {
            return new RbfsResult(null, int.MaxValue);
        }

        successors.Sort();

        while (true)
        {
            var best = successors[0];
            var alternative_f = (successors.Count > 1) ? successors[1].F_Cost : int.MaxValue;

            if (best.F_Cost > f_limit)
            {
                return new RbfsResult(null, best.F_Cost);
            }

            var result = RecursiveSearch(
                best,
                Math.Min(f_limit, alternative_f),
                depth + 1,
                maxIterations
            );

            if (result.Node != null)
            {
                return result;
            }
            best.F_Cost = result.BackedUp_F_Cost;

            successors.Sort();
        }
    }
}