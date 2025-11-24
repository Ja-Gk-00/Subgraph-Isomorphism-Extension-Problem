using System;
using System.Collections.Generic;
using System.IO;

internal record ParsedArgs(
    FileInfo InputFile,
    bool Check,
    bool Extend,
    bool Visualize,
    string SubAlgorithm,
    string ExtensionAlgorithm);

internal class ArgParser
{
    public ParsedArgs? Parse(string[] args)
    {
        string? inputPath = null;
        bool check = false;
        bool extend = false;
        bool visualize = false;
        string subAlgo = "ullmann";
        string extAlgo = "reuse";

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-f":
                case "--file":
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing value for --file");
                        return null;
                    }
                    inputPath = args[++i];
                    break;

                case "--check":
                    check = true;
                    break;

                case "--extend":
                    extend = true;
                    break;

                case "--visualize":
                    visualize = true;
                    break;

                case "--subalgo":
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing value for --subalgo");
                        return null;
                    }
                    subAlgo = args[++i];
                    break;

                case "--extalgo":
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing value for --extalgo");
                        return null;
                    }
                    extAlgo = args[++i];
                    break;

                case "--help":
                case "-h":
                    PrintHelp();
                    return null;

                default:
                    Console.WriteLine($"Unknown argument: {arg}");
                    PrintHelp();
                    return null;
            }
        }

        if (inputPath == null || !File.Exists(inputPath))
        {
            Console.WriteLine("Input file not specified or does not exist.");
            PrintHelp();
            return null;
        }

        if (!check && !extend)
        {
            check = true;
        }

        return new ParsedArgs(
            new FileInfo(inputPath),
            check,
            extend,
            visualize,
            subAlgo,
            extAlgo);
    }

    private void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  --file <path>       Input file with two adjacency matrices");
        Console.WriteLine("  --check             Run subgraph check");
        Console.WriteLine("  --extend            Extend Graph2 to contain Graph1 if needed");
        Console.WriteLine("  --visualize         Produce DOT/PNG visualizations");
        Console.WriteLine("  --subalgo <name>    Subgraph algorithm: ullmann | naive | deg");
        Console.WriteLine("  --extalgo <name>    Extension algorithm: reuse | simple");
        Console.WriteLine("  --help, -h          Show this help");
    }
}
