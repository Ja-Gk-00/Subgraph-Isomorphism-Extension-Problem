using System;
using System.IO;

/// <summary>
/// Main entry point for the application.
/// This class is responsible for parsing arguments (or setting configuration)
/// and coordinating the GraphGenerator to produce the output file(s).
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // --- 1. Configuration Setup ---
        var config = new GeneratorConfig
        {
            N1 = 5,
            N2 = 5,
            Isomorphic = true,
            Density = 0.4,
            MaxWeight = 1,
            Undirected = true,
            AllowLoops = false,
            Seed = 42,
            OutputPath = Path.Combine("data", "isomorphic_set.txt"),
            ExtraData = "Optional comment here.",
            NGraphs = 1
        };

        Console.WriteLine($"Starting batch generation of {config.NGraphs} file(s)...");

        try
        {
            // --- 2. Initialization ---
            var generator = new GraphGenerator(config.Seed);

            // --- 3. Validation ---
            if (config.Isomorphic && config.N1 != config.N2)
            {
                Console.Error.WriteLine("ERROR: Cannot generate isomorphic graphs of different sizes.");
                return; // Exit the program
            }

            // Get base path components
            string? directory = Path.GetDirectoryName(config.OutputPath);
            string baseFilename = Path.GetFileNameWithoutExtension(config.OutputPath);
            string extension = Path.GetExtension(config.OutputPath);
            string fullDirectory = string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : Path.GetFullPath(directory);
            Directory.CreateDirectory(fullDirectory);

            // --- 4. Generation Loop ---
            for (int i = 0; i < config.NGraphs; i++)
            {
                // --- 4a. Determine Current Filename ---
                string currentFilename;
                if (config.NGraphs <= 1)
                {
                    // If only 1 file, use the exact name
                    currentFilename = baseFilename + extension;
                }
                else
                {
                    // Appends "_1", "_2", etc. (using 1-based index)
                    int fileNumber = i + 1;
                    currentFilename = $"{baseFilename}_{fileNumber}{extension}";
                }

                string currentOutputPath = Path.Combine(fullDirectory, currentFilename);
                Console.WriteLine($"Generating file {i + 1}/{config.NGraphs}: {currentOutputPath}...");

                // --- 4b. Generation ---
                // This logic is now inside the loop,
                // so we get fresh graphs for each file.
                int[,] graph1 = generator.GenerateAdjacencyMatrix(
                    config.N1, config.Density, config.MaxWeight,
                    config.Undirected, config.AllowLoops);

                int[,] graph2;

                if (config.Isomorphic)
                {
                    int[] permutation = generator.GeneratePermutation(config.N1);
                    graph2 = generator.CreateIsomorphicGraph(graph1, permutation);
                }
                else
                {
                    graph2 = generator.GenerateAdjacencyMatrix(
                        config.N2, config.Density, config.MaxWeight,
                        config.Undirected, config.AllowLoops);
                }

                // --- 4c. File Writing ---
                generator.WriteGraphFile(currentOutputPath, graph1, graph2, config.ExtraData);
            } // --- End of loop ---

            Console.WriteLine($"\nFile generation complete. {config.NGraphs} file(s) created in:");
            Console.WriteLine(fullDirectory);
        }
        catch (Exception ex)
        {
            // Catch any potential errors (e.g., file permissions, invalid args)
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.ResetColor();
        }
    }
}