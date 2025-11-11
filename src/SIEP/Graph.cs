internal sealed class SIEPException : Exception
{
    public SIEPException()
    { }

    public SIEPException(string message)
        : base(message)
    { }

    public SIEPException(string message, Exception innerException)
        : base(message, innerException)
    { }
}

internal class Graph
{
    private int[,] adjMatrix;
    public int VertexCount
    {
        get
        {
            return adjMatrix.GetLength(0);
        }
    }

    public int EdgeCount { get; private set; }

    public Graph(int[,] adjMatrix)
    {
        this.adjMatrix = adjMatrix;
        Update();
    }

    private void Update()
    {
        foreach (int elem in adjMatrix)
        {
            EdgeCount += elem;
        }
    }
}

internal static class GraphParser
{
    public static IReadOnlyList<Graph> ParseGraphs(FileInfo file)
    {
        var contents = File.ReadLines(file.FullName);
        var msg = $"Input file {file.Name}:";

        if (!contents.Any())
        {
            throw new SIEPException($"{msg} empty");
        }

        var graphs = new List<Graph>();
        var lines = contents.GetEnumerator();

        for (int i = 0; i < 2; i++)
        {
            if (!lines.MoveNext())
            {
                throw new SIEPException($"{msg} enough data");
            }

            var adjMatrix = ParseAdjMatrix(lines, msg);
            graphs.Add(new Graph(adjMatrix));
        }

        if (lines.MoveNext() && string.IsNullOrWhiteSpace(lines.Current))
        {
            throw new SIEPException($"{msg} read all but still some data left");
        }

        return graphs;
    }

    private static int[,] ParseAdjMatrix(IEnumerator<string> lines, string msg)
    {
        int vertexCount;
        try
        {
            vertexCount = int.Parse(lines.Current);
        }
        catch (FormatException exc)
        {
            throw new SIEPException($"{msg} can't parse vertex count", exc);
        }

        if (vertexCount <= 0)
        {
            throw new SIEPException($"{msg} invalid vertex count: {vertexCount}");
        }

        int[,] adjMatrix = new int[vertexCount, vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            if (!lines.MoveNext())
            {
                throw new SIEPException($"{msg} missing rows");
            }

            var line = lines.Current.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (line.Length != vertexCount)
            {
                throw new SIEPException($"{msg} missing columns");
            }

            for (int j = 0; j < vertexCount; j++)
            {
                try
                {
                    adjMatrix[i, j] = int.Parse(line[j]);
                }
                catch (FormatException exc)
                {
                    throw new SIEPException($"{msg} can't parse at {i},{j}", exc);
                }

                if (adjMatrix[i, j] < 0)
                {
                    throw new SIEPException($"{msg} bad value at {i},{j} - {adjMatrix[i, j]}");
                }
            }
        }

        return adjMatrix;
    }
}
