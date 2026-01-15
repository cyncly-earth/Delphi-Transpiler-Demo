
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        Directory.CreateDirectory(outputDir);

        var lines = File.ReadAllLines(irFile);
        var entityName = ParseEntityName(lines);
        var properties = ParseProperties(lines);
        var logicBlocks = ParseLogicBlocks(lines);

        if (string.IsNullOrWhiteSpace(entityName))
        {
            Console.WriteLine("Entity Name not found in IR.");
            return;
        }

        // Generate Angular files
        var tsPath = Path.Combine(outputDir, $"{ToKebab(entityName)}.component.ts");
        var htmlPath = Path.Combine(outputDir, $"{ToKebab(entityName)}.component.html");

        File.WriteAllText(tsPath, GenerateTs(entityName, properties, logicBlocks));
        File.WriteAllText(htmlPath, GenerateHtml(properties));

        Console.WriteLine($"✅ Generated: {tsPath}");
        Console.WriteLine($"✅ Generated: {htmlPath}");
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

    static List<(string Name, string Type)> ParseProperties(string[] lines)
    {
        var props = new List<(string, string)>();
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
                    props.Add((name, type));
                }
            }
        }
        return props;
    }

    static List<(string Name, List<string> Lines)> ParseLogicBlocks(string[] lines)
    {
        var blocks = new List<(string, List<string>)>();
        string currentName = null;
        var currentLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith("Found View Procedure:", StringComparison.OrdinalIgnoreCase))
            {
                if (currentName != null && currentLines.Count > 0)
                {
                    blocks.Add((currentName, new List<string>(currentLines)));
                    currentLines.Clear();
                }
                currentName = line.Split(':')[1].Trim();
            }
            else if (line.TrimStart().StartsWith("Lines Generated"))
            {
                continue;
            }
            else if (line.TrimStart().StartsWith("-") || line.Contains("✓"))
            {
                continue;
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                currentLines.Add(line.Trim());
            }
        }

        if (currentName != null && currentLines.Count > 0)
        {
            blocks.Add((currentName, currentLines));
        }

        return blocks;
    }

    static string GenerateTs(string entityName, List<(string Name, string Type)> props, List<(string Name, List<string> Lines)> logicBlocks)
    {
        var className = ToPascal(entityName) + "Component";
        var cols = string.Join(", ", props.Select(p => $"'{ToCamel(p.Name)}'"));
        var sample = props.Select(p => $"{ToCamel(p.Name)}: {GuessSampleValue(p.Type)}");

        var sb = new StringBuilder();
        sb.AppendLine("import { Component } from '@angular/core';");
        sb.AppendLine("import { CommonModule } from '@angular/common';");
        sb.AppendLine();
        sb.AppendLine($"@Component({{");
        sb.AppendLine($"  selector: 'app-{ToKebab(entityName)}',");
        sb.AppendLine("  standalone: true,");
        sb.AppendLine("  imports: [CommonModule],");
        sb.AppendLine($"  templateUrl: './{ToKebab(entityName)}.component.html'");
        sb.AppendLine("})");
        sb.AppendLine($"export class {className} {{");
        sb.AppendLine($"  items = [{{ {string.Join(", ", sample)} }}];");
        sb.AppendLine($"  columns = [{cols}];");
        sb.AppendLine();

        foreach (var block in logicBlocks)
        {
            sb.AppendLine($"  {ToCamel(block.Name)}(): void {{");
            foreach (var line in block.Lines)
            {
                sb.AppendLine("    " + ConvertToTs(line));
            }
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    static string GenerateHtml(List<(string Name, string Type)> props)
    {
        var th = string.Join(Environment.NewLine, props.Select(p => $"        <th>{ToTitle(p.Name)}</th>"));
        var td = string.Join(Environment.NewLine, props.Select(p => $"        <td>{{{{ item.{ToCamel(p.Name)} }}}}</td>"));
        return $@"<section>
  <h2>{ToTitle(props.First().Name)} List</h2>
  <table>
    <thead>
      <tr>
{th}
      </tr>
    </thead>
    <tbody>
      <tr *ngFor=""let item of items"">
{td}
      </tr>
    </tbody>
  </table>
</section>";
    }

    static string ConvertToTs(string line)
    {
        return line.Replace("==", "===")
                   .Replace("or", "||")
                   .Replace("and", "&&")
                   .Replace(";", ";");
    }

    static string ToCamel(string s) => char.ToLowerInvariant(s[0]) + s.Substring(1);
    static string ToPascal(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    static string ToTitle(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    static string ToKebab(string s) => string.Concat(s.Select((ch, i) => char.IsUpper(ch) && i > 0 ? "-" + char.ToLowerInvariant(ch) : char.ToLowerInvariant(ch).ToString()));

    static string GuessSampleValue(string type)
    {
        type = type.ToLowerInvariant();
        if (type.Contains("int") || type.Contains("decimal")) return "0";
        if (type.Contains("date")) return "new Date()";
        return "''";
    }
}
