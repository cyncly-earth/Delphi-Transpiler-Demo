// using DelphiTranspiler.Ast;

// var builder = new PersonAstBuilder();
// // Path relative to your workspace root
// builder.Build("./run/input/PersonController.pas", "./output");
using DelphiTranspiler.Ast;
using System.IO;

// 1. Get the directory where the .csproj or project root is (up one level from /ast)
string rootDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName 
                 ?? Directory.GetCurrentDirectory();

// 2. Define absolute paths to avoid "File Not Found" errors
string inputPath = Path.Combine(rootDir, "run", "input", "PersonController.pas");
string outputPath = Path.Combine(rootDir, "output");

//Console.WriteLine($"[Pod 1] Looking for input at: {inputPath}");

var builder = new PersonAstBuilder();

if (File.Exists(inputPath))
{
    builder.Build(inputPath, outputPath);
    Console.WriteLine(" Successfully generated the output");
}
else
{
    Console.WriteLine("[Error] Input file not found. Please ensure the file exists in /run/input/");
}