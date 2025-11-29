using System.Collections.Generic;

internal static class GraphExtensionUtils
{
    internal static void ComputeExtensionFromPatternToTarget(
        Graph pattern,
        Graph target,
        Dictionary<int, int> mapping,
        out int addedVertices,
        out int addedEdges)
    {
        int n1 = pattern.VertexCount;
        int n2 = target.VertexCount;

        var mapped = new bool[n2];
        foreach (var kvp in mapping)
        {
            mapped[kvp.Value] = true;
        }

        int mappedCount = mapping.Count;
        addedVertices = n2 - mappedCount;

        long edgeUnits = 0;

        for (int i = 0; i < n1; i++)
        {
            int ti = mapping[i];
            for (int j = i; j < n1; j++)
            {
                int tj = mapping[j];

                int w1 = pattern.Get(i, j);
                int w2 = target.Get(ti, tj);

                int need = w2 - w1;
                if (need > 0)
                    edgeUnits += need;
            }
        }

        for (int u = 0; u < n2; u++)
        {
            for (int v = u; v < n2; v++)
            {
                bool bothMapped = mapped[u] && mapped[v];
                if (bothMapped) continue;

                int w2 = target.Get(u, v);
                if (w2 > 0)
                    edgeUnits += w2;
            }
        }

        addedEdges = (int)edgeUnits;
    }
}
