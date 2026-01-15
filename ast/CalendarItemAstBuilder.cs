using System.IO;
using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DelphiGrammar;

public static class CalendarItemAstBuilder
{
    private static string GetBasePath()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Delphi-Transpiler-Demo.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }
    public static AstUnit Build()
    {
        string basePath = GetBasePath();
        string inputPath = Path.Combine(basePath, "run", "result", "antlr", "input", "classCalendarItem.pas");

        var source = File.ReadAllText(inputPath);
        var inputStream = new AntlrInputStream(source);
        var lexer = new DelphiLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DelphiParser(tokens);

        var tree = parser.file();
        var listener = new CalendarItemAstListener();
        var walker = new ParseTreeWalker();
        walker.Walk(listener, tree);

        return listener.Unit;
    }

    public static void Run()
    {
        string basePath = GetBasePath();
        string outputPath = Path.Combine(basePath, "result", "ast_output", "CalendarItem.ast");
        var unit = Build();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        AstSerializer.Save(unit, outputPath);
    }
}
