namespace pa_lab6;
public class GeneticSolver
{
    private Graph _graph;
    private GeneticParams _params;
    private Random _rnd = new Random();

    public GeneticSolver(Graph graph, GeneticParams parameters)
    {
        _graph = graph;
        _params = parameters;
    }

    public int Run()
    {
        List<bool[]> population = new List<bool[]>();

        // 1. Ініціалізація (НАЇВНА/ВИПАДКОВА)
        for (int i = 0; i < _params.PopSize; i++)
        {
            var ind = new bool[_graph.N];
            for (int j = 0; j < _graph.N; j++) ind[j] = _rnd.NextDouble() > 0.5;
            foreach (var edge in _graph.Edges)
            {
                if (!ind[edge.u] && !ind[edge.v])
                {
                    if (_rnd.NextDouble() > 0.5) ind[edge.u] = true;
                    else ind[edge.v] = true;
                }
            }
            population.Add(ind);
        }

        int globalBestFitness = int.MaxValue;
        
        // Знаходимо кращого серед стартових
        foreach(var ind in population)
        {
            int fit = CalculateFitness(ind);
            if (fit < globalBestFitness) globalBestFitness = fit;
        }

        //Цикл еволюції
        for (int g = 0; g < _params.Generations; g++)
        {
            population = population.OrderBy(CalculateFitness).ToList();

            int currentBestFit = CalculateFitness(population[0]);
            if (currentBestFit < globalBestFitness)
            {
                globalBestFitness = currentBestFit;
            }
            List<bool[]> newPop = [(bool[])population[0].Clone()];

            while (newPop.Count < _params.PopSize)
            {
                var p1 = Tournament(population);
                var p2 = Tournament(population);
                var (c1, c2) = Crossover(p1, p2);

                Mutate(c1); Mutate(c2);
                
                Repair(c1); Prune(c1);
                Repair(c2); Prune(c2);

                newPop.Add(c1);
                if (newPop.Count < _params.PopSize) newPop.Add(c2);
            }
            population = newPop;
        }

        return globalBestFitness;
    }

    private int CalculateFitness(bool[] ind)
    {
        int count = 0;
        for (int i = 0; i < ind.Length; i++) if (ind[i]) count++;
        return count;
    }

    private bool[] Tournament(List<bool[]> pop)
    {
        int k = 3;
        var best = pop[_rnd.Next(pop.Count)];
        for (int i = 0; i < k - 1; i++)
        {
            var challenger = pop[_rnd.Next(pop.Count)];
            if (CalculateFitness(challenger) < CalculateFitness(best)) best = challenger;
        }
        return best;
    }

    private (bool[], bool[]) Crossover(bool[] p1, bool[] p2)
    {
        if (_rnd.NextDouble() > _params.CrossRate) 
            return ((bool[])p1.Clone(), (bool[])p2.Clone());

        // Одноточкове (для прикладу)
        int pt = _rnd.Next(1, _graph.N - 1);
        var c1 = new bool[_graph.N];
        var c2 = new bool[_graph.N];
        Array.Copy(p1, 0, c1, 0, pt);
        Array.Copy(p2, pt, c1, pt, _graph.N - pt);
        Array.Copy(p2, 0, c2, 0, pt);
        Array.Copy(p1, pt, c2, pt, _graph.N - pt);
        return (c1, c2);
    }

    private void Mutate(bool[] ind)
    {
        if (_rnd.NextDouble() < _params.MutRate)
        {
            int idx = _rnd.Next(_graph.N);
            ind[idx] = !ind[idx];
        }
    }

    private void Repair(bool[] ind)
    {
        // Якщо ребро непокрите, покриваємо його
        foreach (var edge in _graph.Edges)
        {
            if (!ind[edge.u] && !ind[edge.v])
            {
                // Евристика: беремо вершину з більшим степенем
                if (_graph.Adjacency[edge.u].Count > _graph.Adjacency[edge.v].Count)
                    ind[edge.u] = true;
                else
                    ind[edge.v] = true;
            }
        }
    }

    private void Prune(bool[] ind)
    {
        // Якщо вершина покрита, але всі її сусіди теж покриті, вона може бути зайвою
        for (int i = 0; i < _graph.N; i++)
        {
            if (ind[i])
            {
                bool canRemove = true;
                foreach (var neighbor in _graph.Adjacency[i])
                {
                    if (!ind[neighbor]) // Сусід не покритий, значить ми єдині хто тримає це ребро
                    {
                        canRemove = false;
                        break;
                    }
                }
                if (canRemove) ind[i] = false;
            }
        }
    }
}