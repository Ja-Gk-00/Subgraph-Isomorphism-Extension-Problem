namespace SIEP.ExtensionAlgorithms;

internal sealed class SimpleCopyExtensionStrategy : IGraphExtensionStrategy
{
    public string Name => "simple";

    public Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges)
    {
        var extended = target.Clone();
        int offset = extended.VertexCount;
        addedVertices = pattern.VertexCount;
        addedEdges = 0;

        for (int i = 0; i < pattern.VertexCount; i++)
            extended.AddVertex();

        for (int i = 0; i < pattern.VertexCount; i++)
        {
            for (int j = i; j < pattern.VertexCount; j++)
            {
                int w = pattern.Get(i, j);
                if (w > 0)
                {
                    int u = offset + i;
                    int v = offset + j;
                    int current = extended.Get(u, v);
                    int delta = w - current;
                    if (delta > 0)
                    {
                        extended.AddEdge(u, v, delta);
                        addedEdges++;
                    }
                }
            }
        }

        return extended;
    }
}
