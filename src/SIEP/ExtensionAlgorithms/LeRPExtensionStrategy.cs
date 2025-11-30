using System;
using System.Collections.Generic;

namespace SIEP.ExtensionAlgorithms;

internal sealed class LeRPExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "lerp";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var matcher = new LeRPMatcher(pattern, target);

        if (matcher.TryMatch(out var mapping, out var exactSubgraph) && exactSubgraph)
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

internal sealed class LeRPMatcher
{
    private const int MaxR = 6;

    private readonly Graph pattern;
    private readonly Graph target;
    private readonly int n1;
    private readonly int n2;
    private readonly int N;

    private readonly long[][,] powersPattern;
    private readonly long[][,] powersTarget;
    private readonly double[,] betaPeak;

    public LeRPMatcher(Graph pattern, Graph target)
    {
        this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        this.target = target ?? throw new ArgumentNullException(nameof(target));

        n1 = pattern.VertexCount;
        n2 = target.VertexCount;
        N = Math.Max(n1, n2);

        powersPattern = ComputeAdjacencyPowers(pattern, MaxR);
        powersTarget = ComputeAdjacencyPowers(target, MaxR);
        betaPeak = ComputeBestBeta(pattern, target, powersPattern, powersTarget, N);
    }

    public bool TryMatch(out Dictionary<int, int> mapping, out bool exactSubgraph)
    {
        mapping = new Dictionary<int, int>();
        exactSubgraph = false;

        if (n1 == 0 || n2 == 0) return false;
        if (n1 > n2) return false;

        mapping = BuildMapping(pattern, target, powersPattern, powersTarget, betaPeak, N);

        if (mapping.Count != n1)
            return false;

        if (!VerifySubgraph(pattern, target, mapping))
            return false;

        exactSubgraph = true;
        return true;
    }

    private static long[][,] ComputeAdjacencyPowers(Graph g, int R)
    {
        int n = g.VertexCount;
        var result = new long[R + 1][,];

        var baseA = new long[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int w = g.Get(i, j);
                baseA[i, j] = (w > 0 || i == j) ? 1L : 0L;
            }
        }

        result[1] = baseA;

        for (int r = 2; r <= R; r++)
        {
            var prev = result[r - 1];
            var next = new long[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int k = 0; k < n; k++)
                {
                    long sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sum += prev[i, j] * baseA[j, k];
                        if (sum < 0)
                        {
                            sum = long.MaxValue;
                            break;
                        }
                    }
                    next[i, k] = sum;
                }
            }

            result[r] = next;
        }

        return result;
    }

    private static double Compare(
        long[][,] powersG,
        long[][,] powersH,
        int i,
        int j,
        int k,
        int l,
        int R,
        int N)
    {
        int rMax = 0;

        for (int r = 1; r <= R; r++)
        {
            if (powersG[r][i, j] == powersH[r][k, l])
                rMax = r;
            else
                break;
        }

        if (rMax == 0 || N == 0) return 0.0;

        double x = (double)rMax / N;
        return x * x;
    }

    private static double[,] ComputeBestBeta(
        Graph pattern,
        Graph target,
        long[][,] powersPattern,
        long[][,] powersTarget,
        int N)
    {
        int n1 = pattern.VertexCount;
        int n2 = target.VertexCount;
        var betaPeak = new double[n1, n2];

        for (int i = 0; i < n1; i++)
        {
            for (int k = 0; k < n2; k++)
            {
                double best = 0.0;

                for (int j = 0; j < n1; j++)
                {
                    if (pattern.Get(i, j) <= 0) continue;

                    for (int l = 0; l < n2; l++)
                    {
                        if (target.Get(k, l) <= 0) continue;

                        double beta = Compare(
                            powersPattern,
                            powersTarget,
                            i,
                            j,
                            k,
                            l,
                            MaxR,
                            N);

                        if (beta > best)
                            best = beta;
                    }
                }

                betaPeak[i, k] = best;
            }
        }

        return betaPeak;
    }

    private static Dictionary<int, int> BuildMapping(
        Graph pattern,
        Graph target,
        long[][,] powersPattern,
        long[][,] powersTarget,
        double[,] betaPeak,
        int N)
    {
        int n1 = pattern.VertexCount;
        int n2 = target.VertexCount;

        var mapping = new Dictionary<int, int>();
        var mappedPattern = new bool[n1];
        var mappedTarget = new bool[n2];

        int maxSteps = Math.Min(n1, n2);

        for (int step = 0; step < maxSteps; step++)
        {
            double peak = 0.0;
            int bestI = -1;
            int bestK = -1;

            for (int i = 0; i < n1; i++)
            {
                if (mappedPattern[i]) continue;

                for (int k = 0; k < n2; k++)
                {
                    if (mappedTarget[k]) continue;

                    if (!IsConsistent(pattern, target, i, k, mapping)) continue;

                    double rho = 0.0;

                    foreach (var kvp in mapping)
                    {
                        int j = kvp.Key;
                        int l = kvp.Value;

                        if (pattern.Get(i, j) <= 0) continue;

                        double beta = Compare(
                            powersPattern,
                            powersTarget,
                            i,
                            j,
                            k,
                            l,
                            MaxR,
                            N);

                        double gamma = Compare(
                            powersPattern,
                            powersTarget,
                            j,
                            j,
                            l,
                            l,
                            MaxR,
                            N);

                        rho = 1.0 - (1.0 - rho) * (1.0 - beta) * (1.0 - gamma);
                    }

                    double alpha = Compare(
                        powersPattern,
                        powersTarget,
                        i,
                        i,
                        k,
                        k,
                        MaxR,
                        N);

                    double bp = betaPeak[i, k];

                    rho = 1.0 - (1.0 - rho) * (1.0 - alpha) * (1.0 - bp);

                    if (rho > peak)
                    {
                        peak = rho;
                        bestI = i;
                        bestK = k;
                    }
                }
            }

            if (peak <= 0.0 || bestI < 0 || bestK < 0)
                break;

            mapping[bestI] = bestK;
            mappedPattern[bestI] = true;
            mappedTarget[bestK] = true;
        }

        return mapping;
    }

    private static bool IsConsistent(
        Graph pattern,
        Graph target,
        int p,
        int t,
        Dictionary<int, int> mapping)
    {
        foreach (var kvp in mapping)
        {
            int j = kvp.Key;
            int tj = kvp.Value;

            int w1 = pattern.Get(p, j);
            if (w1 <= 0) continue;

            int w2 = target.Get(t, tj);
            if (w2 < w1) return false;
        }

        return true;
    }

    private static bool VerifySubgraph(
        Graph pattern,
        Graph target,
        Dictionary<int, int> mapping)
    {
        int n1 = pattern.VertexCount;

        for (int i = 0; i < n1; i++)
        {
            if (!mapping.ContainsKey(i)) return false;
        }

        for (int i = 0; i < n1; i++)
        {
            for (int j = i; j < n1; j++)
            {
                int w1 = pattern.Get(i, j);
                if (w1 <= 0) continue;

                int ti = mapping[i];
                int tj = mapping[j];

                int w2 = target.Get(ti, tj);
                if (w2 < w1) return false;
            }
        }

        return true;
    }
}
