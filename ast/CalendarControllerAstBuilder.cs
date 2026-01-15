using System.IO;
using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DelphiGrammar;

public static class CalendarControllerAstBuilder
{
    private static string GetBasePath()
    {
        // Find the repository root by looking for Delphi-Transpiler-Demo.sln
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Delphi-Transpiler-Demo.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }

    // Build the AstUnit by parsing the original .pas source and walking the parse tree
    public static AstUnit Build()
    {
        string basePath = GetBasePath();
        string inputPath = Path.Combine(basePath, "run", "result", "antlr", "input", "CalendarController.pas");

        var source = File.ReadAllText(inputPath);
        var inputStream = new AntlrInputStream(source);
        var lexer = new DelphiLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DelphiParser(tokens);

        var tree = parser.file(); // entry rule

        var listener = new CalendarControllerAstListener();
        var walker = new ParseTreeWalker();
        walker.Walk(listener, tree);

        return listener.Unit;
    }

    // Keep the serializer for debugging/artifacts
    public static void Run()
    {
        string basePath = GetBasePath();
        string outputPath = Path.Combine(basePath, "result", "ast_output", "CalendarController.ast");
        var unit = Build();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        AstSerializer.Save(unit, outputPath);
    }
}
