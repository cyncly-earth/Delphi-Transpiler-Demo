// /workspaces/Delphi-Transpiler-Demo/ast/ast_runner.cs
// SEPARATE PROGRAM - Doesn't interfere with main Program.cs

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DelphiTranspiler.AST
{
    public class AstRunner
    {
        public static void RunAst(string[] args)
        {
            Console.WriteLine("=== AST Builder (Standalone) ===");
            Console.WriteLine("Building AST from parse trees...\n");
            
            // Input and output directories
            string baseDir = Directory.GetCurrentDirectory();
            string inputDir = Path.Combine(baseDir, "run", "output");  // Parse trees are here
            string outputDir = Path.Combine(baseDir, "run", "output"); // Output AST here
            
            // Ensure directories exist
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);
            
            var builder = new AstBuilder();
            
            // Process each parse tree file
            string[] inputFiles = 
            {
                "classPerson.parse.txt",
                "PersonView.parse.txt", 
                "PersonController.parse.txt"
            };
            
            int successCount = 0;
            int errorCount = 0;
            
            foreach (var inputFile in inputFiles)
            {
                string inputPath = Path.Combine(inputDir, inputFile);
                
                if (File.Exists(inputPath))
                {
                    Console.WriteLine($"\n[{successCount + errorCount + 1}/{inputFiles.Length}] Processing: {inputFile}");
                    
                    try
                    {
                        // Read parse tree
                        string parseTree = File.ReadAllText(inputPath);
                        
                        if (string.IsNullOrWhiteSpace(parseTree))
                        {
                            Console.WriteLine("  ⚠ Warning: Empty parse tree file");
                            errorCount++;
                            continue;
                        }
                        
                        // Build AST
                        var ast = builder.BuildFromParseTree(parseTree, inputFile);
                        
                        // Serialize to JSON
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };
                        
                        string json = JsonSerializer.Serialize(ast, options);
                        
                        // Save to output
                        string outputFile = Path.GetFileNameWithoutExtension(inputFile) + ".ast.json";
                        string outputPath = Path.Combine(outputDir, outputFile);
                        
                        File.WriteAllText(outputPath, json);
                        
                        Console.WriteLine($"  ✓ Created: {outputFile}");
                        Console.WriteLine($"    Size: {json.Length} bytes");
                        
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"    Inner: {ex.InnerException.Message}");
                        }
                        errorCount++;
                    }
                }
                else
                {
                    Console.WriteLine($"\n  ✗ File not found: {inputFile}");
                    Console.WriteLine($"    Looked in: {inputDir}");
                    errorCount++;
                }
            }
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("=== AST Generation Complete ===");
            Console.WriteLine($"✓ Successful: {successCount}");
            Console.WriteLine($"✗ Failed: {errorCount}");
            Console.WriteLine($"📁 Output directory: {outputDir}");
            
            if (successCount > 0)
            {
                Console.WriteLine("\nGenerated files:");
                foreach (var file in inputFiles)
                {
                    string outputFile = Path.GetFileNameWithoutExtension(file) + ".ast.json";
                    string outputPath = Path.Combine(outputDir, outputFile);
                    if (File.Exists(outputPath))
                    {
                        var fileInfo = new FileInfo(outputPath);
                        Console.WriteLine($"  • {outputFile} ({fileInfo.Length} bytes)");
                    }
                }
                
                // Show a sample of the first file
                string sampleFile = Path.GetFileNameWithoutExtension(inputFiles[0]) + ".ast.json";
                string samplePath = Path.Combine(outputDir, sampleFile);
                if (File.Exists(samplePath))
                {
                    Console.WriteLine("\nSample of first AST (first 200 chars):");
                    string sampleContent = File.ReadAllText(samplePath);
                    int previewLength = Math.Min(200, sampleContent.Length);
                    Console.WriteLine($"\"{sampleContent.Substring(0, previewLength)}...\"");
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
