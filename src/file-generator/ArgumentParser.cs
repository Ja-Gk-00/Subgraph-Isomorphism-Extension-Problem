using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A simple, custom parser for command-line arguments
/// that populates a GeneratorConfig record.
/// 
/// This parser expects:
/// 1. Required positional arguments: <n1> <n2>
/// 2. Optional named arguments: --key value (e.g., --density 0.5)
/// 3. Optional flag arguments: --flag (e.g., --isomorphic)
/// </summary>
public static class ArgumentParser
{
    /// <summary>
    /// Parses the command-line arguments array.
    /// </summary>
    /// <param name="args">The string[] args from Program.Main</param>
    /// <returns>A populated GeneratorConfig record.</returns>
    /// <exception cref="ArgumentException">Thrown if arguments are invalid or missing.</exception>
    public static GeneratorConfig Parse(string[] args)
    { // <-- Poprawiona linia, __ zostało zastąpione przez {
        // Start with the default configuration
        var config = new GeneratorConfig();
        var positionalArgs = new List<string>();

        if (args.Length == 0)
        {
            PrintHelp();
            throw new ArgumentException("No arguments provided. Please provide at least n1 and n2.");
        }

        // --- 1. Main Parsing Loop ---
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // A simple --help check
            if (arg == "--help" || arg == "-h")
            {
                PrintHelp();
                // We throw an exception to stop execution.
                // In a real app, you might use a different flow (e.g., return null).
                throw new OperationCanceledException("Help requested.");
            }

            if (arg.StartsWith("--"))
            {
                // This is an optional argument
                // We use 'ref i' to allow the helper method to advance
                // the loop index if it consumes the next argument as a value.
                config = ParseOptionalArgument(ref i, args, config);
            }
            else
            {
                // This is a positional argument (n1 or n2)
                positionalArgs.Add(arg);
            }
        }

        // --- 2. Post-Parsing Validation ---

        // Apply positional arguments
        if (positionalArgs.Count < 2)
        {
            throw new ArgumentException($"Missing required positional arguments. Expected <n1> and <n2>, but got {positionalArgs.Count}.");
        }

        try
        {
            config = config with
            {
                N1 = int.Parse(positionalArgs[0]),
                N2 = int.Parse(positionalArgs[1])
            };
        }
        catch (FormatException)
        {
            throw new ArgumentException($"Invalid format for n1 ('{positionalArgs[0]}') or n2 ('{positionalArgs[1]}'). Both must be integers.");
        }

        return config;
    }

    /// <summary>
    /// Helper method to parse a single optional argument.
    /// </summary>
    private static GeneratorConfig ParseOptionalArgument(ref int i, string[] args, GeneratorConfig currentConfig)
    {
        string key = args[i];

        // This is a "switch expression" (modern C#)
        // It modifies the config record using the 'with' keyword.
        return key switch
        {
            // --- Flags (no value) ---
            "--isomorphic" => currentConfig with { Isomorphic = true },
            "--allow-loops" => currentConfig with { AllowLoops = true },
            "--directed" => currentConfig with { Undirected = false },

            // --- Arguments with values ---
            "--output" => currentConfig with { OutputPath = GetValue(ref i, args) },
            "--extra-data" => currentConfig with { ExtraData = GetValue(ref i, args) },
            "--density" => currentConfig with { Density = GetDouble(ref i, args, key) },
            "--max-weight" => currentConfig with { MaxWeight = GetInt(ref i, args, key) },
            "--seed" => currentConfig with { Seed = GetInt(ref i, args, key) },
            "--ngraphs" => currentConfig with { NGraphs = GetInt(ref i, args, key) },

            // --- Unknown ---
            _ => throw new ArgumentException($"Unknown argument: {key}")
        };
    }

    // --- Helper methods for type conversion and error handling ---

    private static string GetValue(ref int i, string[] args)
    {
        if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
        {
            throw new ArgumentException($"Missing value for argument: {args[i]}");
        }
        i++; // Consume the value
        return args[i];
    }

    private static int GetInt(ref int i, string[] args, string key)
    {
        string value = GetValue(ref i, args);
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        throw new ArgumentException($"Invalid integer value for {key}: '{value}'");
    }

    private static double GetDouble(ref int i, string[] args, string key)
    {
        string value = GetValue(ref i, args);
        // Use InvariantCulture to parse "0.5" not "0,5"
        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        throw new ArgumentException($"Invalid double value for {key}: '{value}'");
    }

    /// <summary>
    /// Prints a help message to the console.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine("Graph Generator Usage:");
        Console.WriteLine("  dotnet run -- <n1> <n2> [options]");
        Console.WriteLine();
        Console.WriteLine("Required Arguments:");
        Console.WriteLine("  n1                Number of vertices for the first graph.");
        Console.WriteLine("  n2                Number of vertices for the second graph.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --isomorphic      Generate G2 as an isomorphism of G1 (requires n1=n2).");
        Console.WriteLine("  --directed        Generate directed graphs (default is undirected).");
        Console.WriteLine("  --allow-loops     Allow self-loops (edges on the matrix diagonal).");
        Console.WriteLine("  --density <0-1>   Set graph density (default: 0.4).");
        Console.WriteLine("  --max-weight <n>  Set max edge weight (default: 1, for simple graph).");
        Console.WriteLine("  --ngraphs <n>     Number of files to generate (default: 1).");
        Console.WriteLine("  --seed <n>        Set random seed for reproducible results.");
        Console.WriteLine("  --output <path>   Base output file path (default: graphs_example.txt).");
        Console.WriteLine("  --extra-data <s>  String to append at the end of the file.");
        Console.WriteLine("  --help, -h        Show this help message.");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  dotnet run -- 10 10 --isomorphic --density 0.25 --ngraphs 5 --output data/test_set.txt");
    }
}