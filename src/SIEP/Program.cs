var parser = new ArgParser();
var o = parser.Parse(args);

if (o is null)
{
    return 1;
}
try
{
    var graphs = GraphParser.ParseGraphs(o.inputFile);

    for (int i = 0; i < graphs.Count; i++)
    {
        Console.WriteLine($"[{i}] graph has {graphs[i].VertexCount} verticies and {graphs[i].EdgeCount} edges");
    }
}
catch (SIEPException exc)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERROR]: {exc.Message}");
    Console.ForegroundColor = orig;

    Console.WriteLine($"\nTrace:\n{exc.StackTrace}");
}
return 0;
