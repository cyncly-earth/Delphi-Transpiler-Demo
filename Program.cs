using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                "run/input/classPerson.parse.txt",
                "run/input/PersonController.parse.txt",
                "run/input/PersonView.parse.txt"
            };

            var outputDir = Path.Combine("run", "output");
            Directory.CreateDirectory(outputDir);

            Console.WriteLine("=== CONVERTING PARSE TREES TO AST ===\n");

            var builder = new NewAstBuilder();
            int successCount = 0;
            int errorCount = 0;

            foreach (var parseTreeFile in inputs)
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
