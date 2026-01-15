using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run -- <ir-file> <output-dir>");
            return;
        }

        var irFile = args[0];
        var outputDir = args[1];

        if (!File.Exists(irFile))
        {
            Console.WriteLine($"IR file not found: {irFile}");
            return;
        }

        // Parse the semantic tree from the IR file
        var semanticTree = ParseSemanticTreeFromFile(irFile);

        // Create the generator and generate Angular components
        var generator = new SemanticToAngularGenerator();
        generator.GenerateAngularComponents(semanticTree, outputDir);
    }

    /// <summary>
    /// Parses semantic tree from an IR file.
    /// This is a helper method for the main entry point.
    /// The semantic team will provide the semantic tree as a C# object directly.
    /// </summary>
    static SemanticToAngularGenerator.SemanticTree ParseSemanticTreeFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var semanticTree = new SemanticToAngularGenerator.SemanticTree
        {
            EntityName = ParseEntityName(lines),
            Properties = ParseProperties(lines),
            LogicBlocks = ParseLogicBlocks(lines)
        };

        return semanticTree;
    }

    static string ParseEntityName(string[] lines)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith("Entity Name:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Split(':')[1].Trim();
            }
        }
        return string.Empty;
    }

    static List<SemanticToAngularGenerator.SemanticProperty> ParseProperties(string[] lines)
    {
        var props = new List<SemanticToAngularGenerator.SemanticProperty>();
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("-", StringComparison.Ordinal))
            {
                var parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    var name = parts[0].Replace("-", "").Trim();
                    var typePart = parts[1].Trim();
                    var type = typePart.Replace("Type:", "").Trim();
                    props.Add(new SemanticToAngularGenerator.SemanticProperty { Name = name, Type = type });
                }
            }
        }
        return props;
    }

    static List<SemanticToAngularGenerator.SemanticLogicBlock> ParseLogicBlocks(string[] lines)
    {
        var blocks = new List<SemanticToAngularGenerator.SemanticLogicBlock>();
        string currentName = null;
        var currentLines = new List<string>();
        bool skipCurrent = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("Found View Procedure:", StringComparison.OrdinalIgnoreCase))
            {
                if (currentName != null && currentLines.Count > 0 && !skipCurrent)
                {
                    blocks.Add(new SemanticToAngularGenerator.SemanticLogicBlock 
                    { 
                        Name = currentName, 
                        Lines = new List<string>(currentLines) 
                    });
                }
                currentLines.Clear();
                currentName = line.Split(':')[1].Trim();
                skipCurrent = false;
            }
            else if (line.Contains("Skipped", StringComparison.OrdinalIgnoreCase))
            {
                skipCurrent = true;
            }
            else if (line.Contains("✓ Added Logic Block", StringComparison.OrdinalIgnoreCase))
            {
                skipCurrent = false;
            }
            else if (line.TrimStart().StartsWith("Lines Generated"))
            {
                continue;
            }
            else if (line.TrimStart().StartsWith("-") || line.Contains("✓"))
            {
                continue;
            }
            else if (!string.IsNullOrWhiteSpace(line) && !skipCurrent)
            {
                currentLines.Add(line.Trim());
            }
        }

        if (currentName != null && currentLines.Count > 0 && !skipCurrent)
        {
            blocks.Add(new SemanticToAngularGenerator.SemanticLogicBlock 
            { 
                Name = currentName, 
                Lines = currentLines 
            });
        }

        return blocks;
    }
}
