namespace SIEP.SubgraphAlgorithms;

using System.Collections.Generic;

internal sealed class NaiveSubgraphSolver : ISubgraphIsomorphismSolver
{
    public string Name => "naive";

    public bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping)
    {
        mapping = new Dictionary<int, int>();

        int n = pattern.VertexCount;
        int m = target.VertexCount;
        if (n > m) return false;

        var indices = new List<int>();
        for (int i = 0; i < m; i++) indices.Add(i);

        var chosen = new int[n];
        return ChooseVertices(pattern, target, indices, 0, chosen, ref mapping);
    }

    private bool ChooseVertices(
        Graph pattern,
        Graph target,
        List<int> available,
        int depth,
        int[] chosen,
        ref Dictionary<int, int> mapping)
    {
        int n = pattern.VertexCount;
        if (depth == n)
        {
            var map = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
                map[i] = chosen[i];

            if (VerifyMapping(pattern, target, map))
            {
                mapping = map;
                return true;
            }
            return false;
        }

        for (int k = 0; k < available.Count; k++)
        {
            int v = available[k];
            chosen[depth] = v;

            var next = new List<int>(available);
            next.RemoveAt(k);

            if (ChooseVertices(pattern, target, next, depth + 1, chosen, ref mapping))
                return true;
        }

        return false;
    }

    private bool VerifyMapping(Graph pattern, Graph target, Dictionary<int, int> mapping)
    {
        int n = pattern.VertexCount;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int w = pattern.Get(i, j);
                if (w > 0)
                {
                    int ti = mapping[i];
                    int tj = mapping[j];
                    if (target.Get(ti, tj) < w)
                        return false;
                }
            }
        }
        return true;
    }
}
