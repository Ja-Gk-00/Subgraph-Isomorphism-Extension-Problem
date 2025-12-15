using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class TreeAugmentationExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "tap";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        if (pattern.VertexCount > target.VertexCount)
            throw new SIEPException("TreeAugmentationExtensionStrategy: pattern has more vertices than target");

        int nT = target.VertexCount;
        int nP = pattern.VertexCount;

        var diffMatrix = new int[nT, nT];
        for (int i = 0; i < nT; i++)
        {
            for (int j = 0; j < nT; j++)
            {
                int t = target.Get(i, j);
                int p = (i < nP && j < nP) ? pattern.Get(i, j) : 0;
                int d = t - p;
                if (d > 0)
                    diffMatrix[i, j] = d;
            }
        }

        var extension = new Graph(diffMatrix);
        int tapAddedEdges = RunTreeAugmentation(extension);

        int size = nT;
        var finalMatrix = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int val = 0;
                if (i < nP && j < nP)
                    val += pattern.Get(i, j);
                val += extension.Get(i, j);
                if (val > 0)
                    finalMatrix[i, j] = val;
            }
        }

        var finalGraph = new Graph(finalMatrix);

        addedVertices = finalGraph.VertexCount - target.VertexCount;
        addedEdges = finalGraph.EdgeCount - target.EdgeCount;

        return finalGraph;
    }

    private int RunTreeAugmentation(Graph g)
    {
        int n = g.VertexCount;
        if (n <= 1) return 0;

        var visited = new bool[n];
        var parent = Enumerable.Repeat(-1, n).ToArray();
        var treeDeg = new int[n];

        for (int s = 0; s < n; s++)
        {
            if (visited[s]) continue;

            var queue = new Queue<int>();
            visited[s] = true;
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                foreach (int u in EnumerateNeighbors(g, v))
                {
                    if (!visited[u])
                    {
                        visited[u] = true;
                        parent[u] = v;
                        treeDeg[u]++;
                        treeDeg[v]++;
                        queue.Enqueue(u);
                    }
                }
            }
        }

        var leaves = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if (treeDeg[i] <= 1)
                leaves.Add(i);
        }

        if (leaves.Count <= 1) return 0;

        int addedEdges = 0;
        int idx = 0;

        while (idx + 1 < leaves.Count)
        {
            int u = leaves[idx];
            int v = leaves[idx + 1];

            if (u != v && g.Get(u, v) == 0)
            {
                g.AddEdge(u, v, 1);
                addedEdges++;
            }

            idx += 2;
        }

        if (idx < leaves.Count && n > 1)
        {
            int last = leaves[idx];

            int best = 0;
            int bestDeg = g.Degree(0);
            for (int i = 1; i < n; i++)
            {
                int d = g.Degree(i);
                if (d > bestDeg && i != last)
                {
                    bestDeg = d;
                    best = i;
                }
            }

            if (best != last && g.Get(last, best) == 0)
            {
                g.AddEdge(last, best, 1);
                addedEdges++;
            }
        }

        return addedEdges;
    }

    private static IEnumerable<int> EnumerateNeighbors(Graph g, int v)
    {
        int n = g.VertexCount;
        for (int u = 0; u < n; u++)
        {
            if (g.Get(v, u) > 0)
                yield return u;
        }
    }
}
