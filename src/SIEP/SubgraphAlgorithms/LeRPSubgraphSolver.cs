using System;
using System.Collections.Generic;

namespace SIEP.SubgraphAlgorithms;

internal sealed class LeRPSubgraphSolver : ISubgraphIsomorphismSolver
{
    public string Name => "lerp";

    private const int MaxR = 6;

    public bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping)
    {
        mapping = new Dictionary<int, int>();

        int n1 = pattern.VertexCount;
        int n2 = target.VertexCount;
        if (n1 == 0 || n2 == 0) return false;

        var powersPattern = ComputeAdjacencyPowers(pattern, MaxR);
        var powersTarget = ComputeAdjacencyPowers(target, MaxR);

        int N = Math.Max(n1, n2);

        var betaPeak = ComputeBestBeta(pattern, target, powersPattern, powersTarget, N);

        mapping = BuildMapping(pattern, target, powersPattern, powersTarget, betaPeak, N);

        if (mapping.Count != n1) return false;

        return VerifySubgraph(pattern, target, mapping);
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
                if (w > 0 || i == j)
                    baseA[i, j] = 1;
                else
                    baseA[i, j] = 0;
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
                        if (sum < 0) sum = long.MaxValue;
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
        int i,
        int k,
        Dictionary<int, int> mapping)
    {
        foreach (var kvp in mapping)
        {
            int j = kvp.Key;
            int l = kvp.Value;

            int w1 = pattern.Get(i, j);
            if (w1 <= 0) continue;

            int w2 = target.Get(k, l);
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
