using System.Text;
using System.Text.Json;
using System.IO;
using System.Linq; 
using System.Collections.Generic;
using DelphiTranspiler.CodeGen.Models; 

public class AngularGenerator
{
    public void Generate(string uiIrJson, string outputDir)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        UiModel? ir = null;

        try { ir = JsonSerializer.Deserialize<UiModel>(uiIrJson, options); } catch {}
        
        // Fallback Logic
        if (ir == null || ir.UiActions == null || ir.UiActions.Count == 0)
        {
             try {
                 var root = JsonSerializer.Deserialize<UiIrRoot>(uiIrJson, options);
                 if (root != null) { ir = new UiModel(); ir.UiActions = root.UiActions; }
             } catch {}
        }

        // Ensure IR and List are never null to satisfy compiler warnings
        if (ir == null) ir = new UiModel();
        if (ir.UiActions == null) ir.UiActions = new List<UiAction>();

        Directory.CreateDirectory(outputDir);

        foreach (var action in ir.UiActions)
        {
            if (action == null) continue;
            GenerateComponent(action, outputDir);
            GenerateHtml(action, outputDir);
            GenerateService(action, outputDir);
            GenerateModel(action, outputDir);
        }
    }

    private void GenerateComponent(UiAction action, string outputDir)
    {
        var className = "AddPersonComponent";
        var sb = new StringBuilder();
        sb.AppendLine("import { Component } from '@angular/core';");
        sb.AppendLine("import { PersonService } from './person.service';");
        sb.AppendLine("import { Person } from './person.model';");
        sb.AppendLine();
        sb.AppendLine("@Component({ selector: 'app-add-person', templateUrl: './add-person.component.html' })");
        sb.AppendLine($"export class {className} {{");
        
        sb.AppendLine("  model: Person = {");
        
        var fields = action.Form?.Fields ?? new List<UiField>();
        var distinctFields = fields.DistinctBy(f => f.Name).ToList();

        foreach (var field in distinctFields) 
            sb.AppendLine($"    {field.Name}: '',");

        sb.AppendLine("  };");
        sb.AppendLine("  constructor(private service: PersonService) {}");
        sb.AppendLine("  submit() { this.service.addPerson(this.model).subscribe(); }");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(outputDir, "add-person.component.ts"), sb.ToString());
    }

    private void GenerateHtml(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<form (ngSubmit)=\"submit()\">");
        
        var fields = action.Form?.Fields ?? new List<UiField>();
        var distinctFields = fields.DistinctBy(f => f.Name).ToList();

        foreach (var field in distinctFields)
        {
            sb.AppendLine("  <div class=\"form-group\">");
            sb.AppendLine($"    <label>{field.Name}</label>");
            sb.AppendLine($"    <input type=\"text\" [(ngModel)]=\"model.{field.Name}\" name=\"{field.Name}\" class=\"form-control\" />");
            sb.AppendLine("  </div>");
        }
        sb.AppendLine("  <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
        sb.AppendLine("</form>");
        File.WriteAllText(Path.Combine(outputDir, "add-person.component.html"), sb.ToString());
    }

    private void GenerateService(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();
        string procName = action.BackendCall?.Procedure ?? "Unknown";

        sb.AppendLine("import { Injectable } from '@angular/core';");
        sb.AppendLine("import { HttpClient } from '@angular/common/http';");
        sb.AppendLine("import { Person } from './person.model';");
        sb.AppendLine("@Injectable({ providedIn: 'root' })");
        sb.AppendLine("export class PersonService {");
        sb.AppendLine("  constructor(private http: HttpClient) {}");
        sb.AppendLine("  addPerson(person: Person) {");
        sb.AppendLine($"    return this.http.post('/api/{procName}', person);");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(outputDir, "person.service.ts"), sb.ToString());
    }

    private void GenerateModel(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("export interface Person {");
        
        var fields = action.Form?.Fields ?? new List<UiField>();
        var distinctFields = fields.DistinctBy(f => f.Name).ToList();

        foreach (var field in distinctFields) 
            sb.AppendLine($"  {field.Name}: string;");
            
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(outputDir, "person.model.ts"), sb.ToString());
    }
}