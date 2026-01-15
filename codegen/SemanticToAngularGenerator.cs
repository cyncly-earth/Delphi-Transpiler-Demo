
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class SemanticToAngularGenerator
{
    public class SemanticProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class SemanticLogicBlock
    {
        public string Name { get; set; }
        public List<string> Lines { get; set; } = new();
    }

    public class SemanticTree
    {
        public string EntityName { get; set; }
        public List<SemanticProperty> Properties { get; set; } = new();
        public List<SemanticLogicBlock> LogicBlocks { get; set; } = new();
    }

    public void GenerateAngularComponents(SemanticTree semanticTree, string outputDir)
    {
        if (string.IsNullOrWhiteSpace(semanticTree.EntityName))
        {
            throw new ArgumentException("Entity Name not found in semantic tree.");
        }

        Directory.CreateDirectory(outputDir);

        // Generate Angular files
        var tsPath = Path.Combine(outputDir, $"{ToKebab(semanticTree.EntityName)}.component.ts");
        var htmlPath = Path.Combine(outputDir, $"{ToKebab(semanticTree.EntityName)}.component.html");

        File.WriteAllText(tsPath, GenerateTs(semanticTree.EntityName, semanticTree.Properties, semanticTree.LogicBlocks));
        File.WriteAllText(htmlPath, GenerateHtml(semanticTree.Properties));

        Console.WriteLine($"✅ Generated: {tsPath}");
        Console.WriteLine($"✅ Generated: {htmlPath}");
    }

    private string GenerateTs(string entityName, List<SemanticProperty> props, List<SemanticLogicBlock> logicBlocks)
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

    private string GenerateHtml(List<SemanticProperty> props)
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

    private string ConvertToTs(string line)
    {
        // Replace comparison operators first (with word boundaries to avoid double conversion)
        line = System.Text.RegularExpressions.Regex.Replace(line, @"([^<>=])===([^=])", "$1===$2"); // Avoid already converted
        
        // Replace Delphi operators with proper order to avoid conflicts
        line = line.Replace(">==", ">=")
                   .Replace("<==", "<=")
                   .Replace("==", "===")
                   .Replace(" or ", " || ")
                   .Replace(" and ", " && ");
        
        return line;
    }

    private string ToCamel(string s) => char.ToLowerInvariant(s[0]) + s.Substring(1);
    private string ToPascal(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    private string ToTitle(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    private string ToKebab(string s) => string.Concat(s.Select((ch, i) => char.IsUpper(ch) && i > 0 ? "-" + char.ToLowerInvariant(ch) : char.ToLowerInvariant(ch).ToString()));

    private string GuessSampleValue(string type)
    {
        type = type.ToLowerInvariant();
        if (type.Contains("int") || type.Contains("decimal")) return "0";
        if (type.Contains("date")) return "new Date()";
        return "''";
    }
}
