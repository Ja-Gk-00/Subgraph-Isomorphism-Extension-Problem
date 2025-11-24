namespace SIEP.SubgraphAlgorithms;

using System.Collections.Generic;

internal interface ISubgraphIsomorphismSolver
{
    string Name { get; }
    bool TryFindSubgraph(Graph pattern, Graph target, out Dictionary<int, int> mapping);
}
