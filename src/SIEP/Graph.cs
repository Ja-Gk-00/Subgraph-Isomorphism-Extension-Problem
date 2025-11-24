internal class Graph
{
    private int[,] adj;

    public int VertexCount => adj.GetLength(0);
    public int EdgeCount { get; private set; }

    public Graph(int[,] matrix)
    {
        adj = matrix;
        EdgeCount = 0;
        foreach (var w in matrix) EdgeCount += w;
    }

    public Graph(int size)
    {
        adj = new int[size, size];
        EdgeCount = 0;
    }

    public void AddVertex()
    {
        int n = VertexCount;
        var newAdj = new int[n + 1, n + 1];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                newAdj[i, j] = adj[i, j];
        adj = newAdj;
    }

    public void AddEdge(int u, int v, int weight)
    {
        adj[u, v] += weight;
        adj[v, u] += weight;
        EdgeCount += 2 * weight;
    }

    public int Get(int i, int j) => adj[i, j];
    public int Degree(int i)
    {
        int d = 0;
        for (int j = 0; j < VertexCount; j++) d += adj[i, j];
        return d;
    }

    public Graph Clone()
    {
        int[,] clone = new int[VertexCount, VertexCount];
        for (int i = 0; i < VertexCount; i++)
            for (int j = 0; j < VertexCount; j++)
                clone[i, j] = adj[i, j];
        return new Graph(clone);
    }

    public void PrintMatrix()
    {
        System.Console.WriteLine(VertexCount);
        for (int i = 0; i < VertexCount; i++)
        {
            for (int j = 0; j < VertexCount; j++)
            {
                System.Console.Write(adj[i, j]);
                if (j < VertexCount - 1) System.Console.Write(" ");
            }
            System.Console.WriteLine();
        }
    }
}
