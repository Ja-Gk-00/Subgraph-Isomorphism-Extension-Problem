using System;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Provides methods to generate and save graph adjacency matrices.
/// </summary>
public class GraphGenerator
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the GraphGenerator.
    /// </summary>
    /// <param name="seed">A specific seed for the random number generator
    /// to ensure reproducible results. If null, a random seed is used.</param>
    public GraphGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Generates a random adjacency matrix based on the specified parameters.
    /// </summary>
    /// <param name="size">Number of vertices (N).</param>
    /// <param name="density">Probability (0.0-1.0) of an edge existing.</param>
    /// <param name="maxWeight">Max edge weight (1 for simple graph, >1 for weighted).</param>
    /// <param name="undirected">True for a symmetric matrix (undirected graph).</param>
    /// <param name="allowLoops">True to allow values on the main diagonal.</param>
    /// <returns>A 2D integer array (N x N) representing the adjacency matrix.</returns>
    public int[,] GenerateAdjacencyMatrix(int size, double density, int maxWeight, bool undirected, bool allowLoops)
    {
        if (density < 0.0 || density > 1.0)
            throw new ArgumentException("Density must be between 0.0 and 1.0.", nameof(density));
        if (maxWeight < 1)
            throw new ArgumentException("MaxWeight must be 1 or greater.", nameof(maxWeight));

        int[,] matrix = new int[size, size];

        for (int i = 0; i < size; i++)
        {
            // For undirected graphs, we only iterate the upper triangle (j >= i)
            // to ensure symmetry and avoid double-processing.
            int jStart = undirected ? i : 0;

            for (int j = jStart; j < size; j++)
            {
                // Handle self-loops (main diagonal)
                if (!allowLoops && i == j)
                {
                    matrix[i, j] = 0;
                    continue;
                }

                // Decide whether to create an edge based on density
                if (_random.NextDouble() <= density)
                {
                    // If an edge exists, assign a random weight
                    // For a simple graph (maxWeight=1), this will always be 1.
                    int weight = _random.Next(1, maxWeight + 1);
                    matrix[i, j] = weight;

                    // If undirected and not on the diagonal, set the symmetric element
                    if (undirected && i != j)
                    {
                        matrix[j, i] = weight;
                    }
                }
                // Note: No 'else' needed, as matrix is initialized with 0.
            }
        }
        return matrix;
    }

    /// <summary>
    /// Generates a random permutation array of a given size.
    /// </summary>
    /// <param name="size">The size of the array (e.g., number of vertices).</param>
    /// <returns>An array of integers [0...size-1] in a random order.</returns>
    public int[] GeneratePermutation(int size)
    {
        // Create an ordered array [0, 1, 2, ..., size-1]
        int[] p = Enumerable.Range(0, size).ToArray();

        // Shuffle the array using the Fisher-Yates algorithm (modern version)
        for (int i = size - 1; i > 0; i--)
        {
            int j = _random.Next(0, i + 1); // Random index from [0, i]
            // Swap elements
            (p[i], p[j]) = (p[j], p[i]);
        }
        return p;
    }

    /// <summary>
    /// Creates a new graph that is isomorphic to the original,
    /// by applying a vertex permutation.
    /// </summary>
    /// <param name="originalMatrix">The original adjacency matrix (G1).</param>
    /// <param name="permutation">The permutation array (P).</param>
    /// <returns>A new adjacency matrix (G2) where A2[i, j] = A1[P[i], P[j]].</returns>
    public int[,] CreateIsomorphicGraph(int[,] originalMatrix, int[] permutation)
    {
        int size = originalMatrix.GetLength(0);
        if (size != permutation.Length)
            throw new ArgumentException("Matrix size and permutation length must match.");

        int[,] newMatrix = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // The new edge (i, j) exists if the old edge (P[i], P[j]) existed.
                newMatrix[i, j] = originalMatrix[permutation[i], permutation[j]];
            }
        }
        return newMatrix;
    }

    /// <summary>
    /// Writes the two graphs to a file according to the specified format.
    /// </summary>
    /// <param name="path">The full path of the file to write.</param>
    /// <param name="matrix1">The first graph's adjacency matrix.</param>
    /// <param name="matrix2">The second graph's adjacency matrix.</param>
    /// <param name="extraData">Optional extra data to append at the end.</param>
    public void WriteGraphFile(string path, int[,] matrix1, int[,] matrix2, string? extraData)
    {
        // Use StreamWriter for efficient file writing
        using (var writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            // --- Graph 1 ---
            writer.WriteLine(matrix1.GetLength(0)); // N1
            WriteMatrix(writer, matrix1); // Matrix for G1

            // --- Graph 2 ---
            writer.WriteLine(matrix2.GetLength(0)); // N2
            WriteMatrix(writer, matrix2); // Matrix for G2

            // --- Optional Extra Data ---
            if (!string.IsNullOrEmpty(extraData))
            {
                // Write extra data (can be multi-line if the string contains \n)
                writer.Write(extraData);
            }
        }
    }

    /// <summary>
    /// Private helper method to write a matrix to the stream
    /// in the required "1 0 1" space-delimited format.
    /// </summary>
    private void WriteMatrix(StreamWriter writer, int[,] matrix)
    {
        int size = matrix.GetLength(0);
        // Use StringBuilder for high-performance string concatenation per-row
        var rowBuilder = new StringBuilder();

        for (int i = 0; i < size; i++)
        {
            rowBuilder.Clear();
            for (int j = 0; j < size; j++)
            {
                rowBuilder.Append(matrix[i, j]);
                if (j < size - 1)
                {
                    rowBuilder.Append(' '); // Space separator
                }
            }
            writer.WriteLine(rowBuilder.ToString());
        }
    }

    /// <summary>
    /// Extracts an induced subgraph by taking the first 'newSize' vertices.
    /// This is the "cut" operation.
    /// </summary>
    /// <param name="originalMatrix">The original graph matrix.</param>
    /// <param name="newSize">The size (N) of the subgraph to extract.</param>
    /// <returns>A new N x N adjacency matrix (the top-left corner).</returns>
    public int[,] GetInducedSubgraph(int[,] originalMatrix, int newSize)
    {
        int originalSize = originalMatrix.GetLength(0);
        if (newSize > originalSize)
        {
            throw new ArgumentException("New size cannot be larger than the original matrix.", nameof(newSize));
        }
        if (newSize < 0)
        {
            throw new ArgumentException("New size must be non-negative.", nameof(newSize));
        }

        int[,] subMatrix = new int[newSize, newSize];
        for (int i = 0; i < newSize; i++)
        {
            for (int j = 0; j < newSize; j++)
            {
                // Simply copy the top-left N x N corner
                subMatrix[i, j] = originalMatrix[i, j];
            }
        }
        return subMatrix;
    }
}