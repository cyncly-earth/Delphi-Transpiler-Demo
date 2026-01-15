using System.Text;
using System.Text.Json;

public class AngularGenerator
{
    public void Generate(string uiIrJson, string outputDir)
    {
        var ir = JsonSerializer.Deserialize<UiIrRoot>(uiIrJson);

        foreach (var action in ir.UiActions)
        {
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
        sb.AppendLine("@Component({");
        sb.AppendLine("  selector: 'app-add-person',");
        sb.AppendLine("  templateUrl: './add-person.component.html'");
        sb.AppendLine("})");
        sb.AppendLine($"export class {className} {{");

        sb.AppendLine("  model: Person = {");

        foreach (var field in action.Form.Fields)
            sb.AppendLine($"    {field.Name}: '',");

        sb.AppendLine("  };");
        sb.AppendLine();
        sb.AppendLine("  constructor(private service: PersonService) {}");
        sb.AppendLine();
        sb.AppendLine("  submit() {");
        sb.AppendLine("    this.service.addPerson(this.model).subscribe();");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        File.WriteAllText($"{outputDir}/add-person.component.ts", sb.ToString());
    }

    private void GenerateHtml(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<form (ngSubmit)=\"submit()\">");

        foreach (var field in action.Form.Fields)
        {
            sb.AppendLine("  <div>");
            sb.AppendLine($"    <label>{field.Name}</label>");
            sb.AppendLine($"    <input [(ngModel)]=\"model.{field.Name}\" name=\"{field.Name}\" />");
            sb.AppendLine("  </div>");
        }

        sb.AppendLine("  <button type=\"submit\">Save</button>");
        sb.AppendLine("</form>");

        File.WriteAllText($"{outputDir}/add-person.component.html", sb.ToString());
    }

    private void GenerateService(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();

        sb.AppendLine("import { Injectable } from '@angular/core';");
        sb.AppendLine("import { HttpClient } from '@angular/common/http';");
        sb.AppendLine("import { Person } from './person.model';");
        sb.AppendLine();
        sb.AppendLine("@Injectable({ providedIn: 'root' })");
        sb.AppendLine("export class PersonService {");
        sb.AppendLine("  constructor(private http: HttpClient) {}");
        sb.AppendLine();
        sb.AppendLine("  addPerson(person: Person) {");
        sb.AppendLine($"    return this.http.post('/api/{action.BackendCall.Procedure}', person);");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        File.WriteAllText($"{outputDir}/person.service.ts", sb.ToString());
    }

    private void GenerateModel(UiAction action, string outputDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("export interface Person {");

        foreach (var field in action.Form.Fields)
            sb.AppendLine($"  {field.Name}: string;");

        sb.AppendLine("}");

        File.WriteAllText($"{outputDir}/person.model.ts", sb.ToString());
    }
}
