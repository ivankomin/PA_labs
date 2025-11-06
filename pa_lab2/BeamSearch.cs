using pa_lab2;

public class BeamSearch
{
    private long _generatedNodes;
    public static List<int[]> GetSuccessors(int[] board)
    {
        List<int[]> successors = new List<int[]>();
        for (int col = 0; col < board.Length; col++)
        {
            int cuurentRow = board[col];
            for (int row = 0; row < board.Length; row++)
            {
                if (row == cuurentRow) continue;

                int[] newBoard = (int[])board.Clone();
                newBoard[col] = row;
                successors.Add(newBoard);
            }
        }
        return successors;
    }
    public SolveResult Solve(int[] initialState, Func<int[], int> heuristicFunc, int k, int maxIterations = 1000)
    {
    _generatedNodes = 0;
        
        var currentBestStates = new List<StateNode>
        {
            new StateNode(initialState, heuristicFunc)
        };
        _generatedNodes++;

        StateNode bestNodeFound = currentBestStates[0];

        for (int i = 0; i < maxIterations; i++)
        {
            if (bestNodeFound.Score == 0)
            {
                return new SolveResult
                {
                    FinalNode = bestNodeFound,
                    Iterations = i,
                    GeneratedNodes = _generatedNodes,
                    NodesInMemory = k
                };
            }

            var allSuccessors = new List<StateNode>();
            foreach (var state in currentBestStates)
            {
                var successors = GetSuccessors(state.Board);
                _generatedNodes += successors.Count;

                foreach (var successorBoard in successors)
                {
                    allSuccessors.Add(new StateNode(successorBoard, heuristicFunc));
                }
            }

            if (allSuccessors.Count == 0)
            {
                return new SolveResult { FinalNode = bestNodeFound, Iterations = i, GeneratedNodes = _generatedNodes, NodesInMemory = k };
            }

            var nextGeneration = allSuccessors
                .OrderBy(node => node.Score)
                .Take(k)
                .ToList();

            if (nextGeneration[0].Score >= bestNodeFound.Score)
            {
                return new SolveResult
                {
                    FinalNode = bestNodeFound,
                    Iterations = i,
                    GeneratedNodes = _generatedNodes,
                    NodesInMemory = k
                };
            }

            currentBestStates = nextGeneration;
            if(currentBestStates[0].Score < bestNodeFound.Score)
            {
                bestNodeFound = currentBestStates[0];
            }
        }
        return new SolveResult
        {
            FinalNode = bestNodeFound,
            Iterations = maxIterations,
            GeneratedNodes = _generatedNodes,
            NodesInMemory = k
        };
    }
}