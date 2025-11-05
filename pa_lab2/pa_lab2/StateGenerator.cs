namespace pa_lab2;

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