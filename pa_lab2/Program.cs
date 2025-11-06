namespace pa_lab2;

public class Program
{
    const int runs = 20;
    const int k = 15;
    const int maxIterationsBeam = 5000;
    const int maxIterationsRbfs = 100000;
    public static void Main(string[] args)
    {
        var beamSolver = new BeamSearch();
        var rbfsSolver = new RBFSSolver();
        List<int[]> states = StateGenerator.GenerateStates(runs);

        Console.WriteLine("\nRBFS, F3 heuristic");
        RunRbfs(rbfsSolver, states, Heuristics.F3_Distance);

        Console.WriteLine("\nRBFS, F2 heuristic");
        RunRbfs(rbfsSolver, states, Heuristics.F2_ConflictingPairs);

        Console.WriteLine("\nBeam, F3 heuristic");
        RunBeam(beamSolver, states, Heuristics.F3_Distance, k);

        Console.WriteLine("\nBeam, F2 heuristic");
        RunBeam(beamSolver, states, Heuristics.F2_ConflictingPairs, k);
    }

    private static void RunRbfs(RBFSSolver solver, List<int[]> states, Func<int[], int> heuristic)
    {
        List<long> iterationsList = new List<long>();
        List<int> deadEndsList = new List<int>();
        List<int> nodesInMemoryList = new List<int>();

        Console.WriteLine("Final State | Success? | Iterations | Nodes in Memory | Total Nodes Generated");

        foreach (var state in states)
        {
            SolveResult result = solver.Solve(state, heuristic, maxIterationsRbfs);

            if (!result.IsSuccess)
            {
                deadEndsList.Add(1);
            }
            iterationsList.Add(result.GeneratedNodes);
            nodesInMemoryList.Add(result.NodesInMemory); 

            string finalStateStr = result.FinalNode != null ? string.Join(", ", result.FinalNode.Board) : "N/A";
            Console.WriteLine($"{finalStateStr} | {result.IsSuccess} | {result.Iterations} | {result.NodesInMemory} | {result.GeneratedNodes}");
        }

        Console.WriteLine("RBFS Statistics:");
        Console.WriteLine($"Average nodes generated: {iterationsList.Average():F2}");
        Console.WriteLine($"Average nodes in memory: {nodesInMemoryList.Average():F2}");
        Console.WriteLine($"Dead ends: {deadEndsList.Count}");
    }

    private static void RunBeam(BeamSearch solver, List<int[]> states, Func<int[], int> heuristic, int k)
    {
        List<long> iterationsList = new List<long>();
        List<int> deadEndsList = new List<int>();
        List<int> nodesInMemoryList = new List<int>();

        Console.WriteLine("Final State | Success? | Iterations | Nodes in Memory | Total Nodes Generated");

        foreach (var state in states)
        {
            SolveResult result = solver.Solve(state, heuristic, k, maxIterationsBeam); 
            if (!result.IsSuccess)
            {
                deadEndsList.Add(1);
            }
            iterationsList.Add(result.Iterations); 
            nodesInMemoryList.Add(result.NodesInMemory); 
            string finalStateStr = result.FinalNode != null ? string.Join(", ", result.FinalNode.Board) : "N/A";
            Console.WriteLine($"{finalStateStr} | {result.IsSuccess} | {result.Iterations} | {result.NodesInMemory} | {result.GeneratedNodes}");
        }

        Console.WriteLine("Beam Statistics:");
        Console.WriteLine($"Average iterations: {iterationsList.Average():F2}");
        Console.WriteLine($"Average nodes in memory: {nodesInMemoryList.Average():F2}");
        Console.WriteLine($"Dead ends: {deadEndsList.Count} (out of {states.Count})");
    }
}