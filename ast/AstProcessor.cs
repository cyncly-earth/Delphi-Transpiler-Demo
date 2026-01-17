using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Transpiler.Semantics; // <--- IMPORTANT

namespace Transpiler.AST
{
    public class AstProcessor
    {
        public void Run(string inputDir, string outputDir)
        {
            // 1. CALL THE PARSER
            List<AstUnit> units = DelphiParser.ParseUnits(inputDir); 

            // 2. Ensure Output Directory
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            // 3. Save Output
            var options = new JsonSerializerOptions { WriteIndented = true };

            foreach (var unit in units)
            {
                string jsonString = JsonSerializer.Serialize(unit, options);
                string fileName = $"{unit.Name}.json";
                File.WriteAllText(Path.Combine(outputDir, fileName), jsonString);
                Console.WriteLine($"[Pod 1] Generated: {fileName}");
            }
        }
    }
}