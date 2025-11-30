using System;
using System.Collections.Generic;

namespace SIEP.ExtensionAlgorithms;

internal sealed class UllmannExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "ullmann";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));
        if (target is null) throw new ArgumentNullException(nameof(target));

        int n1 = pattern.VertexCount;
        int n2 = target.VertexCount;

        if (n1 == 0)
        {
            addedVertices = n2;
            addedEdges = target.EdgeCount;
            return target;
        }

        if (n1 > n2)
            throw new SIEPException("UllmannExtensionStrategy: pattern has more vertices than target");

        var matcher = new UllmannMatcher(pattern, target);
        if (!matcher.TryFindMapping(out var mapping))
            throw new SIEPException("UllmannExtensionStrategy: pattern is not a subgraph of target;");

        GraphExtensionUtils.ComputeExtensionFromPatternToTarget(
            pattern,
            target,
            mapping,
            out addedVertices,
            out addedEdges);

        return target;
    }

    private sealed class UllmannMatcher
    {
        private readonly Graph pattern;
        private readonly Graph target;
        private readonly int n1;
        private readonly int n2;

        private bool[,] M;
        private readonly int[] mapP2T;
        private readonly bool[] usedT;

        public UllmannMatcher(Graph pattern, Graph target)
        {
            this.pattern = pattern;
            this.target = target;
            n1 = pattern.VertexCount;
            n2 = target.VertexCount;

            M = new bool[n1, n2];
            mapP2T = new int[n1];
            usedT = new bool[n2];

            for (int i = 0; i < n1; i++)
                mapP2T[i] = -1;

            InitCompatibilityMatrix();
        }

        private void InitCompatibilityMatrix()
        {
            for (int i = 0; i < n1; i++)
            {
                int degP = pattern.Degree(i);
                for (int j = 0; j < n2; j++)
                {
                    int degT = target.Degree(j);
                    M[i, j] = degT >= degP;
                }
            }

            RefineMatrix();
        }

        private void RefineMatrix()
        {
            bool changed;
            do
            {
                changed = false;

                for (int i = 0; i < n1; i++)
                {
                    for (int j = 0; j < n2; j++)
                    {
                        if (!M[i, j]) continue;
                        bool rowOk = true;

                        for (int ip = 0; ip < n1 && rowOk; ip++)
                        {
                            if (pattern.Get(i, ip) <= 0) continue;

                            bool foundNeighborCandidate = false;
                            for (int jt = 0; jt < n2; jt++)
                            {
                                if (target.Get(j, jt) <= 0) continue;
                                if (M[ip, jt])
                                {
                                    foundNeighborCandidate = true;
                                    break;
                                }
                            }

                            if (!foundNeighborCandidate)
                                rowOk = false;
                        }

                        if (!rowOk)
                        {
                            M[i, j] = false;
                            changed = true;
                        }
                    }
                }
            }
            while (changed);
        }

        public bool TryFindMapping(out Dictionary<int, int> mapping)
        {
            mapping = new Dictionary<int, int>();

            for (int i = 0; i < n1; i++)
            {
                bool any = false;
                for (int j = 0; j < n2; j++)
                {
                    if (M[i, j])
                    {
                        any = true;
                        break;
                    }
                }
                if (!any)
                    return false;
            }

            if (!Search(0))
                return false;

            for (int i = 0; i < n1; i++)
            {
                int t = mapP2T[i];
                if (t >= 0)
                    mapping[i] = t;
            }

            return mapping.Count == n1;
        }

        private bool Search(int depth)
        {
            if (depth == n1)
                return VerifyMapping();

            int row = SelectNextRow();
            if (row < 0) return false;

            for (int j = 0; j < n2; j++)
            {
                if (!M[row, j]) continue;
                if (usedT[j]) continue;

                if (!FeasiblePair(row, j)) continue;

                usedT[j] = true;
                mapP2T[row] = j;

                if (Search(depth + 1))
                    return true;

                mapP2T[row] = -1;
                usedT[j] = false;
            }

            return false;
        }

        private int SelectNextRow()
        {
            int bestRow = -1;
            int bestCount = int.MaxValue;

            for (int i = 0; i < n1; i++)
            {
                if (mapP2T[i] != -1) continue;

                int count = 0;
                for (int j = 0; j < n2; j++)
                {
                    if (M[i, j] && !usedT[j]) count++;
                }

                if (count == 0)
                    return -1;

                if (count < bestCount)
                {
                    bestCount = count;
                    bestRow = i;
                }
            }

            return bestRow;
        }

        private bool FeasiblePair(int i, int j)
        {
            for (int ip = 0; ip < n1; ip++)
            {
                int tj = mapP2T[ip];
                if (tj < 0) continue;

                int wP = pattern.Get(i, ip);
                if (wP > 0)
                {
                    int wT = target.Get(j, tj);
                    if (wT < wP)
                        return false;
                }
            }

            return true;
        }

        private bool VerifyMapping()
        {
            for (int i = 0; i < n1; i++)
            {
                if (mapP2T[i] < 0)
                    return false;
            }

            for (int i = 0; i < n1; i++)
            {
                for (int j = i; j < n1; j++)
                {
                    int wP = pattern.Get(i, j);
                    if (wP <= 0) continue;

                    int ti = mapP2T[i];
                    int tj = mapP2T[j];

                    int wT = target.Get(ti, tj);
                    if (wT < wP)
                        return false;
                }
            }

            return true;
        }
    }
}
