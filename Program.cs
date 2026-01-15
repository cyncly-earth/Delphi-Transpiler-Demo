using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DelphiTranspiler.AST;
using Transpiler.AST;

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

            var outputDir = Path.Combine("run", "output");
            Directory.CreateDirectory(outputDir);

            Console.WriteLine("=== STEP 1: PARSING DELPHI FILES TO PARSE TREES ===\n");

            var parseTreeFiles = new List<string>();

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
                parseTreeFiles.Add(outputFile);

                Console.WriteLine($"  -> Wrote parse output to: {outputFile}\n");
            }

            Console.WriteLine("Done parsing all modules.\n");

            // ============================================
            // STEP 2: CONVERT PARSE TREES TO AST
            // ============================================
            Console.WriteLine("=== STEP 2: CONVERTING PARSE TREES TO AST ===\n");

            var builder = new NewAstBuilder();
            int successCount = 0;
            int errorCount = 0;

            foreach (var parseTreeFile in parseTreeFiles)
            {
                if (!File.Exists(parseTreeFile))
                {
                    Console.WriteLine($"[ERROR] Parse tree file not found: {parseTreeFile}");
                    errorCount++;
                    continue;
                }

                try
                {
                    string parseTreeText = File.ReadAllText(parseTreeFile);
                    string fileName = Path.GetFileNameWithoutExtension(parseTreeFile).Replace(".parse", "");

                    Console.WriteLine($"Building AST from: {parseTreeFile}");

                    // Convert parse tree to AST using new format
                    var ast = builder.BuildFromParseTree(parseTreeText, fileName);

                    // Serialize to JSON
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    string json = JsonSerializer.Serialize(ast, typeof(Transpiler.AST.AstUnit), options);

                    // Save AST to JSON file
                    string astOutputFile = Path.Combine(outputDir, fileName + ".ast.json");
                    File.WriteAllText(astOutputFile, json);

                    Console.WriteLine($"  ✓ Created AST: {astOutputFile}");
                    Console.WriteLine($"    Size: {json.Length} bytes\n");

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error building AST: {ex.Message}\n");
                    errorCount++;
                }
            }

            Console.WriteLine("=== CONVERSION COMPLETE ===");
            Console.WriteLine($"✓ Successful: {successCount}");
            Console.WriteLine($"✗ Failed: {errorCount}");
            Console.WriteLine($"📁 Output directory: {outputDir}");
        }
    }
}
