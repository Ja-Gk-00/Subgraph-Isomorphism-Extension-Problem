using System.Diagnostics;
using SIEP.ExtensionAlgorithms;
using SIEP.SubgraphAlgorithms;

public class Program
{
    public static void Main(string[] args)
    {
        var parser = new ArgParser();
        var options = parser.Parse(args);
        if (options is null) return;

        try
        {
            var graphs = GraphParser.ParseGraphs(options.InputFile);
            var g1 = graphs[0];
            var g2 = graphs[1];

            if (options.Size)
            {
                PrintSize(g1, g2);
            }

            if (options.Distance)
            {
                PrintDistance(g1, g2);
            }

            var solver = CreateSubgraphSolver(options.SubAlgorithm);
            var extender = CreateExtensionStrategy(options.ExtensionAlgorithm);

            if (options.Check)
                Console.WriteLine($"Subgraph solver: {solver.Name}");

            if (options.Extend)
                Console.WriteLine($"Extension strategy: {extender.Name}");

            if (options.Check)
            {
                var found = solver.TryFindSubgraph(g1, g2, out var mapping);
                if (found)
                    Console.WriteLine("Graph1 is a subgraph of Graph2");
                else
                    Console.WriteLine("Graph1 is NOT a subgraph of Graph2");

                if (options.Visualize)
                {
                    Console.WriteLine("[viz] Visualizing Graph1...");
                    GraphVisualizer.Visualize(g1, "graph1");

                    if (found)
                    {
                        var highlight = new HashSet<int>(mapping.Values);
                        Console.WriteLine("[viz] Visualizing Graph2 with highlight...");
                        GraphVisualizer.Visualize(g2, "graph2", highlight);
                    }
                    else
                    {
                        Console.WriteLine("[viz] Visualizing Graph2...");
                        GraphVisualizer.Visualize(g2, "graph2");
                    }
                }

                if (options.Extend)
                {
                    var extended = extender.ExtendToInclude(
                        g1,
                        g2,
                        out var addedVertices,
                        out var addedEdges);

                    Console.WriteLine("Extension result:");
                    Console.WriteLine($"Added vertices: {addedVertices}");
                    Console.WriteLine($"Added edges (multiplicity units): {addedEdges}");
                    Console.WriteLine("Resulting graph adjacency matrix:");
                    extended.PrintMatrix();

                    if (options.Visualize)
                    {
                        Console.WriteLine("[viz] Visualizing extended / target graph...");
                        GraphVisualizer.Visualize(extended, "graph2_extended");
                    }
                }
            }
        }

        catch (SIEPException ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: Unexpected error: {ex.Message}");
        }
    }

    private static ISubgraphIsomorphismSolver CreateSubgraphSolver(string name)
    {
        var n = name.ToLowerInvariant();
        return n switch
        {
            "naive" => new NaiveSubgraphSolver(),
            "deg" => new DegreeOrderedSubgraphSolver(),
            "degree" => new DegreeOrderedSubgraphSolver(),
            _ => new UllmannSubgraphSolver(),
        };
    }

    private static IGraphExtensionStrategy CreateExtensionStrategy(string name)
    {
        var n = name.ToLowerInvariant();
        return n switch
        {
            "simple" => new SimpleCopyExtensionStrategy(),
            "reuse" => new ReuseVerticesExtensionStrategy(),
            "disjoint" => new DisjointCopyExtensionStrategy(),
            "greedy-reuse" => new GreedyReuseExtensionStrategy(),
            "vf2-disjoint" => new VF2DisjointExtensionStrategy(),
            "vf2-reuse" => new VF2ReuseExtensionStrategy(),
            "tap" => new TreeAugmentationExtensionStrategy(),
            _ => new ReuseVerticesExtensionStrategy(),
        };
    }

    private static void PrintSize(Graph graphA, Graph graphB)
    {
        Console.WriteLine($"Graph1 size: {graphA.Size} (|V| = {graphA.VertexCount}, |E| = {graphA.EdgeCount})");
        Console.WriteLine($"Graph2 size: {graphB.Size} (|V| = {graphB.VertexCount}, |E| = {graphB.EdgeCount})");
    }

    private static void PrintDistance(Graph graphA, Graph graphB)
    {
        graphA.EdgesOn();
        graphB.EdgesOn();
        
        var (elapsed, res) = TimeExecution(() => GraphDistance.Calculate(graphA, graphB));
        Console.WriteLine($"Distance Graph1 <-> Graph2: {res:F5} | took {elapsed.TotalMilliseconds:F5}ms");
        
        graphA.EdgesOff();
        graphB.EdgesOff();
    }
    
    private static (TimeSpan, T) TimeExecution<T>(Func<T> methodToRun)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var res = methodToRun.Invoke();
        stopwatch.Stop();
        return (stopwatch.Elapsed, res);
    }
}
