using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DelphiTranspiler.Ast;

public class PersonAstBuilder
{
    public void Build(string inputPath, string outputDir)
    {
        string content = File.ReadAllText(inputPath);
        TranspilerOutput ast = Parse(content);
        WriteOutputFiles(ast, outputDir);
    }

    private TranspilerOutput Parse(string code)
    {
        var output = new TranspilerOutput();

        // 1. Parse Models (Required for classPerson.parse.txt)
        var modelMatches = Regex.Matches(code, @"(?<=uses\s+)[\w, ]+|(?<=:\s*)T\w+", RegexOptions.IgnoreCase);
        foreach (Match m in modelMatches)
        {
            var names = m.Value.Split(',');
            foreach (var name in names) 
                if(!string.IsNullOrWhiteSpace(name)) output.RequiredModels.Add(name.Trim());
        }

        // 2. Parse Methods (Required for PersonController.parse.txt)
        var methodMatches = Regex.Matches(code, @"procedure\s+(\w+)\s*\((.*?)\);", RegexOptions.IgnoreCase);
        foreach (Match m in methodMatches)
        {
            var method = new MethodNode { Name = m.Groups[1].Value };
            var paramText = m.Groups[2].Value;
            if (paramText.Contains(":"))
            {
                var p = paramText.Split(':');
                method.Parameters.Add(new ParamNode(p[0].Trim(), p[1].Trim()));
            }
            output.ControllerMethods.Add(method);
        }

        // 3. Parse View Mappings (Required for PersonView.parse.txt)
        var viewMatches = Regex.Matches(code, @"FieldByName\('(\w+)'\)\.Value\s*:=\s*(\w+)\.(\w+);");
        foreach (Match m in viewMatches)
        {
            output.ViewMappings.Add(new DataMapping(m.Groups[1].Value, m.Groups[3].Value));
        }

        return output;
    }

    private void WriteOutputFiles(TranspilerOutput ast, string dir)
    {
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // File 1: classPerson.parse.txt
        File.WriteAllLines(Path.Combine(dir, "classPerson.parse.txt"), 
            ast.RequiredModels.ConvertAll(m => $"Model Found: {m}"));

        // File 2: PersonController.parse.txt
        var controllerLines = new List<string>();
        foreach(var m in ast.ControllerMethods)
            controllerLines.Add($"Action: {m.Name} | Args: {string.Join(", ", m.Parameters)}");
        File.WriteAllLines(Path.Combine(dir, "PersonController.parse.txt"), controllerLines);

        // File 3: PersonView.parse.txt
        File.WriteAllLines(Path.Combine(dir, "PersonView.parse.txt"), 
            ast.ViewMappings.ConvertAll(v => $"UI_BINDING: {v.FieldName} maps to {v.SourceProperty}"));
    }
}