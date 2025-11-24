namespace SIEP.SubgraphAlgorithms;

using System.Collections.Generic;
using System.Linq;

internal sealed class DegreeOrderedSubgraphSolver : ISubgraphIsomorphismSolver
{
    public string Name => "degree";

    public bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping)
    {
        mapping = new Dictionary<int, int>();

        if (pattern.VertexCount > target.VertexCount)
            return false;

        int n = pattern.VertexCount;
        var order = Enumerable.Range(0, n)
            .OrderByDescending(i => pattern.Degree(i))
            .ToArray();

        var candidates = BuildCandidateMatrix(pattern, target);
        var current = new Dictionary<int, int>();
        var used = new HashSet<int>();

        return MatchRecursive(0, order, pattern, target, candidates, current, used, ref mapping);
    }

    private bool[,] BuildCandidateMatrix(Graph pattern, Graph target)
    {
        int n = pattern.VertexCount;
        int m = target.VertexCount;
        var matrix = new bool[n, m];

        for (int i = 0; i < n; i++)
        {
            int degP = pattern.Degree(i);
            for (int j = 0; j < m; j++)
            {
                if (target.Degree(j) >= degP)
                    matrix[i, j] = true;
            }
        }

        return matrix;
    }

    private bool MatchRecursive(
        int depth,
        int[] order,
        Graph pattern,
        Graph target,
        bool[,] matrix,
        Dictionary<int, int> current,
        HashSet<int> used,
        ref Dictionary<int, int> final)
    {
        int n = pattern.VertexCount;
        if (depth == n)
        {
            if (VerifyMapping(pattern, target, current))
            {
                final = new Dictionary<int, int>(current);
                return true;
            }
            return false;
        }

        int pVertex = order[depth];

        for (int j = 0; j < target.VertexCount; j++)
        {
            if (!matrix[pVertex, j] || used.Contains(j))
                continue;

            current[pVertex] = j;
            used.Add(j);

            if (MatchRecursive(depth + 1, order, pattern, target, matrix, current, used, ref final))
                return true;

            used.Remove(j);
            current.Remove(pVertex);
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
