using SIEP.ExtensionAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class GreedyReuseExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "greedy-reuse";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var extended = target.Clone();
        int initialTargetVertices = extended.VertexCount;

        int n1 = pattern.VertexCount;

        int[] map = new int[n1];
        for (int i = 0; i < n1; i++)
        {
            map[i] = -1;
        }

        var order = Enumerable.Range(0, n1)
            .OrderByDescending(i => pattern.Degree(i))
            .ToArray();

        bool[] usedTarget = new bool[initialTargetVertices];

        foreach (int p in order)
        {
            int degP = pattern.Degree(p);
            int chosen = -1;

            for (int t = 0; t < initialTargetVertices; t++)
            {
                if (usedTarget[t]) continue;
                if (extended.Degree(t) >= degP)
                {
                    chosen = t;
                    break;
                }
            }

            if (chosen == -1)
            {
                extended.AddVertex();
                chosen = extended.VertexCount - 1;
            }
            else
            {
                usedTarget[chosen] = true;
            }

            map[p] = chosen;
        }

        addedVertices = extended.VertexCount - initialTargetVertices;
        addedEdges = 0;

        for (int i = 0; i < n1; i++)
        {
            for (int j = i; j < n1; j++)
            {
                int w = pattern.Get(i, j);
                if (w <= 0) continue;

                int u = map[i];
                int v = map[j];

                int current = extended.Get(u, v);
                if (current < w)
                {
                    int delta = w - current;
                    extended.AddEdge(u, v, delta);
                    addedEdges++;
                }
            }
        }

        return extended;
    }
}
