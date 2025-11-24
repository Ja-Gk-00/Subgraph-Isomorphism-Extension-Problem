using System;
using System.Collections.Generic;
using System.IO;

internal sealed class SIEPException : Exception
{
    public SIEPException() { }

    public SIEPException(string message) : base(message) { }

    public SIEPException(string message, Exception inner) : base(message, inner) { }
}

internal static class GraphParser
{
    public static IReadOnlyList<Graph> ParseGraphs(FileInfo file)
    {
        var lines = File.ReadLines(file.FullName).GetEnumerator();
        var graphs = new List<Graph>();

        for (int k = 0; k < 2; k++)
        {
            if (!lines.MoveNext())
                throw new SIEPException("Missing graph size.");

            if (!int.TryParse(lines.Current, out int size) || size <= 0)
                throw new SIEPException("Invalid vertex count.");

            var matrix = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                if (!lines.MoveNext())
                    throw new SIEPException("Incomplete matrix.");

                var tokens = lines.Current.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != size)
                    throw new SIEPException("Matrix row size mismatch.");

                for (int j = 0; j < size; j++)
                {
                    if (!int.TryParse(tokens[j], out matrix[i, j]) || matrix[i, j] < 0)
                        throw new SIEPException($"Invalid matrix value at {i},{j}.");
                }
            }

            graphs.Add(new Graph(matrix));
        }

        return graphs;
    }
}
