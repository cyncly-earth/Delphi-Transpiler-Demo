using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DelphiGrammar;

namespace DelphiTranspilerDemo
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var inputs = new[]
            {
                "run/input/classCalendarItem.pas",
                "run/input/CalendarView.pas",
                "run/input/CalendarController.pas"
            };

            var outputDir = Path.Combine("run", "output");
            Directory.CreateDirectory(outputDir);

            foreach (var inputPath in inputs)
            {
                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"[WARN] Skipping missing file: {inputPath}");
                    continue;
                }

                Console.WriteLine($"Parsing: {inputPath}");

                // 1. Character stream
                var inputStream = CharStreams.fromPath(inputPath);

                // 2. Lexer
                var lexer = new DelphiLexer(inputStream);

                // 3. Token stream
                var tokens = new CommonTokenStream(lexer);

                // 4. Parser
                var parser = new DelphiParser(tokens);
                parser.BuildParseTree = true;

                // 5. Parse (entry rule = file)
                IParseTree tree = parser.file();

                // 6. Serialize parse tree (debug output)
                var treeText = tree.ToStringTree(parser);

                var outputFile =
                    Path.Combine(
                        outputDir,
                        Path.GetFileNameWithoutExtension(inputPath) + ".parse.txt"
                    );

                File.WriteAllText(outputFile, treeText);

                Console.WriteLine($"  -> Wrote parse output to: {outputFile}");
            }

            Console.WriteLine("Done parsing all modules.");
        }
    }
}
