namespace SIEP.ExtensionAlgorithms;

using System.Collections.Generic;
using System.Linq;

internal sealed class ReuseVerticesExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "reuse";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var extended = target.Clone();
        int targetInitial = extended.VertexCount;
        int n = pattern.VertexCount;

        var map = new int[n];
        for (int i = 0; i < n; i++) map[i] = -1;

        var patternOrder = Enumerable.Range(0, n)
            .OrderByDescending(i => pattern.Degree(i))
            .ToArray();

        var targetUsed = new bool[targetInitial];

        foreach (var pi in patternOrder)
        {
            int degP = pattern.Degree(pi);
            int chosen = -1;

            for (int tj = 0; tj < targetInitial; tj++)
            {
                if (extended.Degree(tj) >= degP && !targetUsed[tj])
                {
                    chosen = tj;
                    break;
                }
            }

            if (chosen == -1)
            {
                extended.AddVertex();
                chosen = extended.VertexCount - 1;
            }

            map[pi] = chosen;
            if (chosen < targetInitial)
                targetUsed[chosen] = true;
        }

        addedVertices = extended.VertexCount - targetInitial;
        addedEdges = 0;

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                int w = pattern.Get(i, j);
                if (w > 0)
                {
                    int u = map[i];
                    int v = map[j];
                    int current = extended.Get(u, v);
                    int delta = w - current;
                    if (delta > 0)
                    {
                        extended.AddEdge(u, v, delta);
                        addedEdges++;
                    }
                }
            }
        }

        return extended;
    }
}
