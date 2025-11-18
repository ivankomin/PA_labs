public static class StateGenerator
{
    static Random random = new Random();
    
    private static int[] GenerateRandomState()
    {
        int[] board = Enumerable.Range(0,8).ToArray();
        random.Shuffle(board);
        return board;
    }

    public static List<int[]> GenerateStates(int count)
    {
        List<int[]> states = new List<int[]>();
        for (int i = 0; i < count; i++)
        {
            states.Add(GenerateRandomState());
        }
        return states;
    }
}
public static class Heuristics
{
    private static readonly int[] solution = { 4, 6, 0, 3, 1, 7, 5, 2 };

    public static int F3_Distance(int[] board)
    {
        int conflicts = 0;
        for (int i = 0; i < solution.Length; i++)
        {
            if (board[i] != solution[i])
            {
                conflicts++;
            }
        }
        return conflicts;
    }
    
    public static int F2_ConflictingPairs(int[] board)
    {
        int conflicts = 0;
        for (int i = 0; i < board.Length; i++)
        {
            for (int j = i + 1; j < board.Length; j++)
            {
                if (board[i] == board[j])
                {
                    conflicts++;
                }
                if (Math.Abs(board[i] - board[j]) == Math.Abs(i - j))
                {
                    conflicts++;
                }
            }
        }
        return conflicts;
    }
}