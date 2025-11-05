using pa_lab2;

public class BeamSearch
{
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
    public StateNode Solve(int[] initialState, Func<int[], int> heuristicFunc, int k, int maxIterations = 1000)
    {
        var currentBestStates = new List<StateNode>
        {
            new StateNode(initialState, heuristicFunc)
        };

        for (int i = 0; i < maxIterations; i++)
        {
            var bestSoFar = currentBestStates[0];
            if (bestSoFar.Score == 0)
            {
                Console.WriteLine($"Solution found at iteration {i}");
                return bestSoFar;
            }

            var allSuccessors = new List<StateNode>();
            foreach (var state in currentBestStates)
            {
                var successors = GetSuccessors(state.Board);
                foreach (var successorBoard in successors)
                {
                    allSuccessors.Add(new StateNode(successorBoard, heuristicFunc));
                }
            }

            var nextGeneration = allSuccessors
                .OrderBy(node => node.Score)
                .Take(k)
                .ToList();

            if (nextGeneration.Count == 0 || nextGeneration[0].Score >= bestSoFar.Score)
            {
                Console.WriteLine($"Local minimum found at iteration {i}. Score = {bestSoFar.Score}");
                return bestSoFar;
            }
            currentBestStates = nextGeneration;
        }

        Console.WriteLine($"Reached max iterations ({maxIterations})");
        return currentBestStates.OrderBy(s => s.Score).First();
    }
}