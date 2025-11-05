namespace pa_lab2;

public class Program
{
    const int runs = 20;
    const int k = 15;
    const int maxIterations = 10000;
    public static void Main(string[] args)
    {
        var solver = new BeamSearch();
        List<int[]> states = StateGenerator.GenerateStates(runs);
        
        /*Console.WriteLine("Local beam search, F3 heuristic");
        foreach (var state in states)
        {
            Console.WriteLine($"\n{states.IndexOf(state) + 1}. State: {string.Join(", ", state)}");
            var solution = solver.Solve(state, Heuristics.F3Distance, k, maxIterations);
            Console.WriteLine($"Score: {solution.Score}, Board: {string.Join(", ", solution.Board)}");
        }*/

        Console.WriteLine("Local beam search, F2 heuristic");
        foreach (var state in states)
        {
            Console.WriteLine($"\n{states.IndexOf(state) + 1}. State: {string.Join(", ", state)}");
            var solution = solver.Solve(state, Heuristics.F2_ConflictingPairs, k, maxIterations);
            Console.WriteLine($"Score: {solution.Score}, Board: {string.Join(", ", solution.Board)}");
        }
    }
}