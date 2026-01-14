
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: dotnet run <input.cs> <component-name> <output-dir>");
            return;
        }

        var inputFile = args[0];
        var componentName = args[1]; // e.g., order-list
        var outputDir = args[2];

        var code = File.ReadAllText(inputFile);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classNode == null)
        {
            Console.WriteLine("No class found in input.");
            return;
        }

        var props = classNode.Members.OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.Text)
            .ToList();

        Directory.CreateDirectory(outputDir);
        var tsPath = Path.Combine(outputDir, $"{componentName}.component.ts");
        var htmlPath = Path.Combine(outputDir, $"{componentName}.component.html");

        File.WriteAllText(tsPath, GenerateTs(componentName, props));
        File.WriteAllText(htmlPath, GenerateHtml(props));

        Console.WriteLine($"✅ Generated: {tsPath}");
        Console.WriteLine($"✅ Generated: {htmlPath}");
    }

    static string GenerateTs(string name, List<string> props)
    {
        var className = ToPascal(name) + "Component";
        var cols = string.Join(", ", props.Select(p => $"'{ToCamel(p)}'"));
        return $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';

@Component({{
  selector: 'app-{name}',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './{name}.component.html'
}})
export class {className} {{
  items = [
    {{ {string.Join(", ", props.Select(p => $"{ToCamel(p)}: ''"))} }}
  ];
  columns = [{cols}];
}}";
    }

    static string GenerateHtml(List<string> props)
    {
        var th = string.Join(Environment.NewLine, props.Select(p => $"        <th>{ToTitle(p)}</th>"));
        var td = string.Join(Environment.NewLine, props.Select(p => $"        <td>{{{{ item.{ToCamel(p)} }}}}</td>"));
        return $@"<section>
  <h2>List</h2>
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

    static string ToCamel(string s) => char.ToLowerInvariant(s[0]) + s.Substring(1);
    static string ToPascal(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    static string ToTitle(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
}