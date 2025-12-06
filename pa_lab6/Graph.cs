namespace pa_lab6;
public class Graph
{
    public int N { get; }
    public List<HashSet<int>> Adjacency { get; }
    public List<(int u, int v)> Edges { get; }

    public Graph(int n, int minDeg, int maxDeg)
    {
        N = n;
        Adjacency = new List<HashSet<int>>(n);
        Edges = new List<(int, int)>();
        for (int i = 0; i < n; i++) Adjacency.Add(new HashSet<int>());

        var rnd = new Random();

        // Гарантуємо мінімальну зв'язність
        for (int i = 1; i < n; i++)
        {
            int p = rnd.Next(0, i);
            AddEdge(i, p);
        }

        // Додаємо ребра до виконання умов minDeg
        var indices = Enumerable.Range(0, n).ToList();
        for (int i = 0; i < n; i++)
        {
            while (Adjacency[i].Count < minDeg)
            {
                int target = indices[rnd.Next(n)];
                if (target != i && !Adjacency[i].Contains(target) && Adjacency[target].Count < maxDeg)
                {
                    AddEdge(i, target);
                }
                else if (Adjacency[i].Count >= maxDeg) break; // Safety break
            }
        }
    }

    private void AddEdge(int u, int v)
    {
        if (!Adjacency[u].Contains(v))
        {
            Adjacency[u].Add(v);
            Adjacency[v].Add(u);
            Edges.Add((u, v));
        }
    }
}

