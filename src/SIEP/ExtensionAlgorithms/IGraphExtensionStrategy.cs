internal interface IGraphExtensionStrategy
{
    string Name { get; }
    Graph ExtendToInclude(Graph pattern, Graph target, out int addedVertices, out int addedEdges);
}
