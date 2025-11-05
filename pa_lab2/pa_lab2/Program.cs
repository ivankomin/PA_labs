namespace pa_lab2;

class Program
{
    private const int stateAmount = 20;
    private readonly int[] finalState = {4, 6, 0, 3, 1, 7, 5, 2};
    
    public static void Main(string[] args)
    {
        List<int[]> states = StateGenerator.GenerateStates(stateAmount);
    }
}