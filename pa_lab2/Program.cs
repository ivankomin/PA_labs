namespace pa_lab2;

public class Program
{
    const int runs = 20;
    const int k = 15;
    const int maxIterationsBeam = 10000;
    const int maxIterationsRbfs = 100000;
    public static void Main(string[] args)
    {
        var beamSolver = new BeamSearch();
        var rbfsSolver = new RBFSSolver();
        List<int[]> states = StateGenerator.GenerateStates(runs);

        /*Console.WriteLine("Local beam search, F3 heuristic");
        foreach (var state in states)
        {
            Console.WriteLine($"\n{states.IndexOf(state) + 1}. State: {string.Join(", ", state)}");
            var solution = solver.Solve(state, Heuristics.F3Distance, k, maxIterations);
            Console.WriteLine($"Score: {solution.Score}, Board: {string.Join(", ", solution.Board)}");
        }*/

        /*Console.WriteLine("Local beam search, F2 heuristic");
        foreach (var state in states)
        {
            Console.WriteLine($"\n{states.IndexOf(state) + 1}. State: {string.Join(", ", state)}");
            var solution = beamSolver.Solve(state, Heuristics.F2_ConflictingPairs, k, maxIterations);
            Console.WriteLine($"Score: {solution.Score}, Board: {string.Join(", ", solution.Board)}");
        }*/
        Console.WriteLine("\nRBFS, F3 heuristic");
        RunRbfs(rbfsSolver, states, Heuristics.F3Distance);

        Console.WriteLine("\nRBFS, F2 heuristic");
        RunRbfs(rbfsSolver, states, Heuristics.F2_ConflictingPairs);
    }
    
    private static void RunRbfs(RBFSSolver solver, List<int[]> states, Func<int[], int> heuristic)
        {
            List<long> iterationsList = new List<long>();
            List<int> deadEndsList = new List<int>();

            Console.WriteLine("State | Success? | Nodes Generated");

            foreach (var state in states)
            {
                SolveResult result = solver.Solve(state, heuristic, maxIterations: 100000); 

                if (!result.IsSuccess)
                {
                    deadEndsList.Add(1);
                }
                iterationsList.Add(result.GeneratedNodes);

                Console.WriteLine($"... | {result.IsSuccess} | {result.GeneratedNodes}");
            }
            
            Console.WriteLine("RBFS Statistics:");
            Console.WriteLine($"Average nodes generated: {iterationsList.Average():F2}");
            Console.WriteLine($"Dead ends: {deadEndsList.Count}");
        }
}