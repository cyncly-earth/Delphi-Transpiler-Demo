using System;
using System.IO;
using DelphiTranspilerDemo.Parsing;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Text.Json;
namespace DelphiTranspilerDemo
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var inputs = new[]
            {
                "run/input/classPerson.pas",
                "run/input/PersonController.pas",
                "run/input/PersonView.pas"
            };

            var ParserService = new DelphiParserService();
            foreach(var inputPath in inputs)
            {
                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"[WARN] Missing: {inputPath}");
                    continue;
                }

                Console.WriteLine($"Parsing: {inputPath}");

                var parseTree = ParserService.ParseFile(inputPath);
                var astBuilder = new AstBuilder();
                ParseTreeWalker.Default.Walk(astBuilder, parseTree);
                var ast = astBuilder.Unit;
                var json = JsonSerializer.Serialize(
    ast,
    new JsonSerializerOptions { WriteIndented = true }
);

//File.WriteAllText("output.ast.json", json);
var outputDir = Path.Combine("run", "output");
Directory.CreateDirectory(outputDir);

var outputFile = Path.Combine(
    outputDir,
    Path.GetFileNameWithoutExtension(inputPath) + ".ast.json"
);

File.WriteAllText(outputFile, json);
                // TEMPORARY: debug only
                //Console.WriteLine(parseTree.ToStringTree());
            }
            Console.WriteLine("Done parsing all modules.");
        }
    }
}
