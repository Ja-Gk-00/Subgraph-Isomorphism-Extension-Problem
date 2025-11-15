using System.CommandLine;

internal record ParsedArgs(FileInfo inputFile);

internal class ArgParser
{
    private Option<FileInfo> optInputFile;
    private RootCommand rootCommand;
    private ParseResult? parseResult;

    public ArgParser()
    {
        rootCommand = new(
            description: "SIEP - Subgraph Isomorphism Extension Problem"
        );

        optInputFile = new(
            name: "--file",
            aliases: ["-f"])
        {
            Description = "input file",
            Arity = ArgumentArity.ExactlyOne,
            Required = true,
        };
        optInputFile.AcceptExistingOnly();
        rootCommand.Add(optInputFile);
    }

    public ParsedArgs? Parse(IReadOnlyList<string> args)
    {
        parseResult = rootCommand.Parse(args);

        var invokeResult = parseResult.Invoke();


        if (invokeResult != 0)
        {
            return null;
        }

        return new ParsedArgs
        (
            inputFile: parseResult.GetValue(optInputFile)!
        );
    }
}
