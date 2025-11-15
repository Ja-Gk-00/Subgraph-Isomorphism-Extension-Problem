using System;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        GeneratorConfig config;
        try
        {
            // --- 1. Parse Arguments ---
            config = ArgumentParser.Parse(args);
        }
        catch (ArgumentException ex)
        {
            // Handle known parsing errors (e.g., missing value)
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Argument Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("Use --help for more information.");
            return;
        }
        catch (OperationCanceledException)
        {
            // This is thrown by PrintHelp() to stop execution
            return;
        }
        catch (Exception ex)
        {
            // Handle other unexpected errors
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.ResetColor();
            return; // Exit
        }

        // --- 2. Run Main Program Logic ---
        Console.WriteLine($"Starting batch generation of {config.NGraphs} file(s)...");

        try
        {
            var generator = new GraphGenerator(config.Seed);

            if (config.Isomorphic && config.N1 != config.N2)
            {
                Console.Error.WriteLine("ERROR: Cannot generate isomorphic graphs of different sizes.");
                return;
            }

            string? directory = Path.GetDirectoryName(config.OutputPath);
            string baseFilename = Path.GetFileNameWithoutExtension(config.OutputPath);
            string extension = Path.GetExtension(config.OutputPath);
            string fullDirectory = string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : Path.GetFullPath(directory);

            Directory.CreateDirectory(fullDirectory);

            for (int i = 0; i < config.NGraphs; i++)
            {
                string currentFilename;
                if (config.NGraphs <= 1)
                {
                    currentFilename = baseFilename + extension;
                }
                else
                {
                    int fileNumber = i + 1;
                    currentFilename = $"{baseFilename}_{fileNumber}{extension}";
                }

                string currentOutputPath = Path.Combine(fullDirectory, currentFilename);
                Console.WriteLine($"Generating file {i + 1}/{config.NGraphs}: {currentOutputPath}...");

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

                generator.WriteGraphFile(currentOutputPath, graph1, graph2, config.ExtraData);
            }

            Console.WriteLine($"\nFile generation complete. {config.NGraphs} file(s) created in:");
            Console.WriteLine(fullDirectory);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.ResetColor();
        }
    }
}