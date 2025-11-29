using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class VF2SubgraphMatcher
{
    private readonly Graph pattern;
    private readonly Graph target;
    private readonly int n1;
    private readonly int n2;

    private readonly int[] mapP2T;
    private readonly int[] mapT2P;
    private readonly int[] order;

    public VF2SubgraphMatcher(Graph pattern, Graph target)
    {
        this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        this.target = target ?? throw new ArgumentNullException(nameof(target));

        n1 = pattern.VertexCount;
        n2 = target.VertexCount;

        mapP2T = Enumerable.Repeat(-1, n1).ToArray();
        mapT2P = Enumerable.Repeat(-1, n2).ToArray();

        order = Enumerable.Range(0, n1)
            .OrderByDescending(v => pattern.Degree(v))
            .ToArray();
    }

    public bool TryFindMapping(out Dictionary<int, int> mapping)
    {
        mapping = new Dictionary<int, int>();
        if (n1 > n2) return false;

        bool success = MatchRecursive(0);
        if (!success) return false;

        for (int p = 0; p < n1; p++)
        {
            int t = mapP2T[p];
            if (t >= 0)
                mapping[p] = t;
        }

        return mapping.Count == n1;
    }

    private bool MatchRecursive(int depth)
    {
        if (depth == n1)
            return true;

        int p = order[depth];

        if (mapP2T[p] != -1)
            return MatchRecursive(depth + 1);

        int degP = pattern.Degree(p);

        for (int t = 0; t < n2; t++)
        {
            if (mapT2P[t] != -1) continue;
            if (target.Degree(t) < degP) continue;
            if (!FeasiblePair(p, t)) continue;

            mapP2T[p] = t;
            mapT2P[t] = p;

            if (MatchRecursive(depth + 1))
                return true;

            mapP2T[p] = -1;
            mapT2P[t] = -1;
        }

        return false;
    }

    private bool FeasiblePair(int p, int t)
    {
        for (int x = 0; x < n1; x++)
        {
            int tx = mapP2T[x];
            if (tx == -1) continue;

            int wP = pattern.Get(p, x);
            if (wP > 0)
            {
                int wT = target.Get(t, tx);
                if (wT < wP)
                    return false;
            }
        }

        return true;
    }
}

internal sealed class VF2DisjointExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "vf2-disjoint";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var vf2 = new VF2SubgraphMatcher(pattern, target);

        if (vf2.TryFindMapping(out var mapping))
        {
            GraphExtensionUtils.ComputeExtensionFromPatternToTarget(
                pattern, target, mapping, out addedVertices, out addedEdges);

            return target;
        }

        var baseStrategy = new DisjointCopyExtensionStrategy();
        var extended = baseStrategy.ExtendToInclude(pattern, target, out var v, out var e);

        addedVertices = v;
        addedEdges = e;
        return extended;
    }
}

internal sealed class VF2ReuseExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "vf2-reuse";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var vf2 = new VF2SubgraphMatcher(pattern, target);

        if (vf2.TryFindMapping(out var mapping))
        {
            GraphExtensionUtils.ComputeExtensionFromPatternToTarget(
                pattern, target, mapping, out addedVertices, out addedEdges);

            return target;
        }

        var baseStrategy = new GreedyReuseExtensionStrategy();
        var extended = baseStrategy.ExtendToInclude(pattern, target, out var v, out var e);

        addedVertices = v;
        addedEdges = e;
        return extended;
    }
}
