using SIEP.ExtensionAlgorithms;

internal sealed class DisjointCopyExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "disjoint";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var extended = target.Clone();
        int offset = extended.VertexCount;
        int n1 = pattern.VertexCount;

        for (int i = 0; i < n1; i++)
        {
            extended.AddVertex();
        }

        addedVertices = n1;
        addedEdges = 0;

        for (int i = 0; i < n1; i++)
        {
            for (int j = i; j < n1; j++)
            {
                int w = pattern.Get(i, j);
                if (w <= 0) continue;

                int u = offset + i;
                int v = offset + j;

                extended.AddEdge(u, v, w);
                addedEdges++;
            }
        }

        return extended;
    }
}
