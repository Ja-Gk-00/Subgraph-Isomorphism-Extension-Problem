using System;
using System.Collections.Generic;
using System.Linq;

namespace SIEP.SubgraphAlgorithms;

internal sealed class VF2SubgraphSolver : ISubgraphIsomorphismSolver
{
    public string Name => "vf2";

    public bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping)
    {
        var matcher = new VF2Matcher(pattern, target);
        return matcher.TryMatch(out mapping);
    }

    private sealed class VF2Matcher
    {
        private readonly Graph pattern;
        private readonly Graph target;
        private readonly int n1;
        private readonly int n2;

        private readonly int[] mapP2T;
        private readonly int[] mapT2P;
        private readonly int[] order;

        public VF2Matcher(Graph pattern, Graph target)
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

        public bool TryMatch(out Dictionary<int, int> mapping)
        {
            mapping = new Dictionary<int, int>();
            if (n1 > n2) return false;

            if (!MatchRecursive(0))
                return false;

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
}
