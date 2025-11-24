using System;
using System.Collections.Generic;

internal class GraphExtender
{
    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var extended = target.Clone();
        int offset = target.VertexCount;
        addedVertices = 0;
        addedEdges = 0;

        var matcher = new GraphMatcher();
        if (matcher.TryFindSubgraph(pattern, target, out _))
        {
            addedVertices = 0;
            addedEdges = 0;
            return extended;
        }

        for (int i = 0; i < pattern.VertexCount; i++)
            extended.AddVertex();

        for (int i = 0; i < pattern.VertexCount; i++)
        {
            for (int j = 0; j < pattern.VertexCount; j++)
            {
                int w = pattern.Get(i, j);
                if (w > 0)
                {
                    extended.AddEdge(offset + i, offset + j, w);
                    addedEdges++;
                }
            }
        }

        addedVertices = pattern.VertexCount;
        return extended;
    }
}
