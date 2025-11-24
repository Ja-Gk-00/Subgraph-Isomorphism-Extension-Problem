using System;
using System.Collections.Generic;

internal class GraphMatcher
{
    public bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping)
    {
        mapping = new();
        if (pattern.VertexCount > target.VertexCount)
            return false;

        var candidates = BuildCandidateMatrix(pattern, target);
        return MatchRecursive(0, pattern, target, candidates, new Dictionary<int, int>(), new HashSet<int>(), ref mapping);
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

    private bool MatchRecursive(int depth, Graph pattern, Graph target, bool[,] matrix, Dictionary<int, int> current, HashSet<int> used, ref Dictionary<int, int> final)
    {
        int n = pattern.VertexCount;
        if (depth == n)
        {
            if (VerifyMapping(pattern, target, current))
            {
                final = new(current);
                return true;
            }
            return false;
        }

        for (int j = 0; j < target.VertexCount; j++)
        {
            if (!matrix[depth, j] || used.Contains(j)) continue;

            current[depth] = j;
            used.Add(j);
            if (MatchRecursive(depth + 1, pattern, target, matrix, current, used, ref final))
                return true;
            used.Remove(j);
            current.Remove(depth);
        }
        return false;
    }

    private bool VerifyMapping(Graph pattern, Graph target, Dictionary<int, int> mapping)
    {
        foreach (var u in mapping.Keys)
            foreach (var v in mapping.Keys)
            {
                if (pattern.Get(u, v) > target.Get(mapping[u], mapping[v]))
                    return false;
            }
        return true;
    }
}
