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

        var matcher = new UllmannMatcher(pattern, target);

        matcher.TryFindMapping(out var mapping);

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

            Search(0);

            for (int i = 0; i < n1; i++)
            {
                int t = mapP2T[i];
                if (t >= 0)
                    mapping[i] = t;
            }

            return mapping.Count > 0;
        }

        private bool Search(int depth)
        {
            if (depth == n1)
                return true;

            int row = SelectNextRow();
            if (row < 0) return false;

            bool anySuccess = false;

            for (int j = 0; j < n2; j++)
            {
                if (!M[row, j]) continue;
                if (usedT[j]) continue;

                if (!FeasiblePair(row, j)) continue;

                usedT[j] = true;
                mapP2T[row] = j;

                if (Search(depth + 1))
                    anySuccess = true;

                mapP2T[row] = -1;
                usedT[j] = false;
            }

            return anySuccess;
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
                    continue;

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
    }
}
