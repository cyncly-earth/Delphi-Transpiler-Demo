using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Transpiler.AST;

namespace DelphiTranspilerDemo
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Program Main invoked (debug)");
            // Intentional compile-time check: remove after verifying build picks up this file
            this_will_not_compile;
            var inputs = new[]
            {
                "run/input/classCalendarItem.pas",
                "run/input/CalendarView.pas",
                "run/input/CalendarController.pas"
            };

            var parseOutputDir = Path.Combine("run", "output");
            var astOutputDir = Path.Combine("result", "ast_output");

            Directory.CreateDirectory(parseOutputDir);
            Directory.CreateDirectory(astOutputDir);

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

                // 5. Parse (entry rule)
                var tree = parser.file();

                // 6. Write parse tree (CST) – keep this for debugging
                var treeText = tree.ToStringTree(parser);
                var parseFile =
                    Path.Combine(
                        parseOutputDir,
                        Path.GetFileNameWithoutExtension(inputPath) + ".parse.txt"
                    );

                File.WriteAllText(parseFile, treeText);
                Console.WriteLine($"  -> Wrote parse output to: {parseFile}");

                // ================================
                // 7. BUILD AST (ONLY FOR CONTROLLER)
                // ================================
                // DEBUG: print filename and equality result
                Console.WriteLine($"  filename: '{Path.GetFileName(inputPath)}' Equals check: {Path.GetFileName(inputPath).Equals("CalendarController.pas", StringComparison.OrdinalIgnoreCase)}");
                if (Path.GetFileName(inputPath)
                        .Equals("CalendarController.pas",
                                StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("  -> Building AST for CalendarController");

                    var builder = new CalendarControllerAstBuilder();
                    AstUnit ast = builder.Build((DelphiParser.FileContext)tree);

                    // Debug print
                    Console.WriteLine("  AST UNIT: " + ast.Name);
                    foreach (var p in ast.Procedures)
                    {
                        Console.WriteLine(
                            $"    {p.Name} ({p.Parameters}) HasBody={p.HasBody}"
                        );
                    }

                    // Save AST
                    var astFile =
                        Path.Combine(astOutputDir, "CalendarController.ast");

                    AstSerializer.Save(ast, astFile);
                    Console.WriteLine($"  -> AST written to: {astFile}");
                }
            }

            Console.WriteLine("Done parsing all modules.");
        }
    }
}
