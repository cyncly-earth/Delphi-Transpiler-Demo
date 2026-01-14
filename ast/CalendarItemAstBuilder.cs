using System.IO;
using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

public static class CalendarItemAstBuilder
{
    public static AstUnit Build()
    {
        string inputPath = @"run/input/classCalendarItem.pas";

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
        string outputPath = @"result/ast_output/CalendarItem.ast";
        var unit = Build();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
        AstSerializer.Save(unit, outputPath);
    }
}
