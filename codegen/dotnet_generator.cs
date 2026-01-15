using System.Text;
using System.Text.RegularExpressions;
using DelphiToCsConverter;

namespace MyTranspiler.Services;

public class BackendCodeGenerator
{
    public Dictionary<string, string> GeneratedFiles { get; } = [];

    public void Generate(SemanticSolution solution)
    {
        GeneratedFiles.Clear();
        foreach (var entity in solution.Entities) GenerateEntity(entity);
        foreach (var service in solution.Services)
        {
            GenerateInterface(service);
            GenerateImplementation(service, solution.LogicBlocks);
        }
    }

    private void GenerateEntity(IrEntity entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        sb.AppendLine("namespace MyBackend.Models;\n");
        sb.AppendLine($"public class {entity.Name}\n{{");
        foreach (var prop in entity.Properties)
        {
            if (prop.IsKey) sb.AppendLine("    [Key]");
            sb.AppendLine($"    public {MapType(prop.Type)} {prop.Name} {{ get; set; }}");
        }
        sb.AppendLine("}");
        GeneratedFiles[$"Models/{entity.Name}.cs"] = sb.ToString();
    }

    private void GenerateImplementation(IrService svc, List<IrLogic> logicBlocks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using MyBackend.Models;\nnamespace MyBackend.Services;\n");
        sb.AppendLine($"public class {svc.Name} : I{svc.Name}\n{{");

        foreach (var method in svc.Methods)
        {
            string signature = method.Signature.Contains("CalendarItem") ? method.Signature : $"{method.Signature}, CalendarItem item";
            sb.AppendLine($"    public async Task {method.Name}Async({signature})\n    {{");
            
            var logic = logicBlocks.FirstOrDefault(l => l.Name.Equals(method.Name, StringComparison.OrdinalIgnoreCase));
            if (logic != null)
                foreach (var line in logic.Lines) sb.AppendLine($"        {RefineSyntax(line)}");
            else
                sb.AppendLine("        await Task.CompletedTask;");

            sb.AppendLine("    }\n");
        }
        sb.AppendLine("}");
        GeneratedFiles[$"Services/{svc.Name}.cs"] = sb.ToString();
    }

    private string RefineSyntax(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";

        // 1. Repair concatenated "else" (Fixes: AccomID-6elseAccomID==8)
        line = Regex.Replace(line, @"(\d)else", "$1; } else { ");

        // 2. .NET 10 Range Fix
        line = Regex.Replace(line, @"if(\w+)in\[(\d+)\.\.(\d+)\]then", m => 
            $"if ({m.Groups[1].Value} is >= {m.Groups[2].Value} and <= {m.Groups[3].Value}) {{ ");

        // 3. Repair comparison to assignment
        if (line.Contains("==") && !line.Contains("if (") && !line.Contains("is"))
            line = line.Replace("==", " = ");

        // 4. Keyword and Context cleanup
        line = Regex.Replace(line, @"\bor\b", " || ");
        line = Regex.Replace(line, @"\band\b", " && ");
        line = line.Replace("Item.", "item.").Replace("Result.", "item.");
        
        // 5. Final Block Polish
        if (line.Contains("{") && !line.Contains("}")) line += "; }";
        if (!line.Trim().EndsWith(";") && !line.Trim().EndsWith("}")) line += ";";

        return line;
    }

    public void SaveToDisk(string rootFolder)
    {
        if (Directory.Exists(rootFolder)) Directory.Delete(rootFolder, true);
        Directory.CreateDirectory(rootFolder);
        foreach (var file in GeneratedFiles)
        {
            string path = Path.Combine(rootFolder, file.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, file.Value);
        }
    }

    private string MapType(string t) => t.ToLower() switch {
        "int" or "integer" => "int",
        "double" or "decimal" => "double",
        _ => "string"
    };

    private void GenerateInterface(IrService svc) 
    {
        var sb = new StringBuilder();
        sb.AppendLine("using MyBackend.Models;");
        sb.AppendLine("namespace MyBackend.Services;\n");
        sb.AppendLine($"public interface I{svc.Name}\n{{");
        foreach (var m in svc.Methods)
            sb.AppendLine($"    Task {m.Name}Async({m.Signature}, CalendarItem item);");
        sb.AppendLine("}");
        GeneratedFiles[$"Services/I{svc.Name}.cs"] = sb.ToString();
    }
}