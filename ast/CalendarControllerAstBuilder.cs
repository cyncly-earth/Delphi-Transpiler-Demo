using System.IO;
using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

public static class CalendarControllerAstBuilder
{
    // Build the AstUnit by parsing the original .pas source and walking the parse tree
    public static AstUnit Build()
    {
        string inputPath = @"run/input/CalendarController.pas";

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
        string outputPath = @"result/ast_output/CalendarController.ast";
        var unit = Build();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
        AstSerializer.Save(unit, outputPath);
    }
}
