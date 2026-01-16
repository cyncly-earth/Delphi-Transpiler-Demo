using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Transpiler.Semantics;

namespace Transpiler.AST
{
    public class AstProcessor
    {
       // Inside ast/AstProcessor.cs

public void Run()
{
    // 1. Define Paths
    string currentDir = Directory.GetCurrentDirectory();
    string inputDirectory = Path.Combine(currentDir, "run", "input");
    string outputDirectory = Path.Combine(currentDir, "output");

    // 2. CALL THE DYNAMIC PARSER
    // Pass the directory so it can find the files automatically
    List<AstUnit> units = DelphiParser.ParseUnits(inputDirectory); 

    // 3. Ensure Output Directory
    if (!Directory.Exists(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
    }

    // 4. Save Output
    var options = new JsonSerializerOptions { WriteIndented = true };

    foreach (var unit in units)
    {
        string jsonString = JsonSerializer.Serialize(unit, options);
        string fileName = $"{unit.Name}.json";
        File.WriteAllText(Path.Combine(outputDirectory, fileName), jsonString);
        Console.WriteLine($"[Pod 1] Parsed & Generated: {fileName}");
    }
}
    }
}