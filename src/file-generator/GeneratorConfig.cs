using System.IO;

/// <summary>
/// Holds all settings for the graph generation process.
/// </summary>
public record GeneratorConfig
{
    // --- Required arguments ---

    /// <summary>
    /// The number of vertices for the first graph.
    /// </summary>
    public int N1 { get; init; }

    /// <summary>
    /// The number of vertices for the second graph.
    /// </summary>
    public int N2 { get; init; }

    /// <summary>
    /// If true, the second graph will be generated
    /// as an isomorphic permutation of the first graph.
    /// If N1 != N2, smaller graph is an isomorphoc subgraph
    /// of the larger graph.
    /// </summary>
    public bool Isomorphic { get; init; } = false;

    // --- Suggested additional arguments ---

    /// <summary>
    /// The file path where the output will be saved.
    /// </summary>
    public string OutputPath { get; init; } = Path.Combine(Directory.GetCurrentDirectory(), "graphs_example.txt");

    /// <summary>
    /// If true, generates an undirected graph (symmetric matrix).
    /// If false, generates a directed graph.
    /// </summary>
    public bool Undirected { get; init; } = true;

    /// <summary>
    /// The probability (0.0 to 1.0) of an edge existing
    /// between any two vertices.
    /// </summary>
    public double Density { get; init; } = 0.4;

    /// <summary>
    /// The maximum weight for an edge.
    /// 1 = Simple graph (0/1).
    /// >1 = Weighted graph / multigraph (0 to MaxWeight).
    /// </summary>
    public int MaxWeight { get; init; } = 1;

    /// <summary>
    /// If true, allows self-loops (edges from a vertex to itself).
    /// These are represented on the matrix diagonal.
    /// </summary>
    public bool AllowLoops { get; init; } = false;

    /// <summary>
    /// An optional string of extra data to be appended
    /// to the end of the file, as per the specification.
    /// </summary>
    public string? ExtraData { get; init; } = null;

    /// <summary>
    /// A specific seed for the random number generator.
    /// Using the same seed ensures reproducible results,
    /// which is excellent for testing and debugging.
    /// If null, a random seed will be used.
    /// </summary>
    public int? Seed { get; init; } = null;

    /// <summary>
    /// Number of files to generate.
    /// Defaults to 1. If greater than 1, file number is apppended to file name.
    /// </summary>
    public int NGraphs { get; init; } = 1;
}