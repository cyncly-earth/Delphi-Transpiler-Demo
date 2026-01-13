using System;
using System.IO;
using Antlr4.Runtime;

class AntlrTest
{
    static void Main()
    {
        var inputText = File.ReadAllText("run/input/BookingForm.pas");

        var inputStream = new AntlrInputStream(inputText);
        var lexer = new DelphiLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DelphiParser(tokens);

        parser.BuildParseTree = true;
        var tree = parser.file();   // IMPORTANT

        Console.WriteLine("Parsing completed successfully.");
    }
}
