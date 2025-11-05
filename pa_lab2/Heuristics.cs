namespace pa_lab2;

public static class Heuristics
{
    private static readonly int[] solution = { 4, 6, 0, 3, 1, 7, 5, 2 };

    public static int F3Distance(int[] board)
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