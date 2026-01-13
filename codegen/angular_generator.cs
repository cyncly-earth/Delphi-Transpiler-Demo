using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace CodeGen
{
    // IR MODELS (match teammate JSON)
    public class AngularIR
    {
        public string ComponentName { get; set; }
        public string ApiRoute { get; set; }
        public List<ActionIR> Actions { get; set; }
    }

    public class ActionIR
    {
        public string Name { get; set; }
        public string Http { get; set; }
    }

    public class AngularGenerator
    {
        public void Generate(string inputJsonPath)
        {
            var ir = JsonSerializer.Deserialize<AngularIR>(
                File.ReadAllText(inputJsonPath)
            );

            var outputDir = Path.Combine("run", "output", "angular");
            Directory.CreateDirectory(outputDir);

            GenerateService(ir, outputDir);
            GenerateComponent(ir, outputDir);
            GenerateTemplate(ir, outputDir);
        }

        private void GenerateService(AngularIR ir, string dir)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import { Injectable } from '@angular/core';");
            sb.AppendLine("import { HttpClient } from '@angular/common/http';");
            sb.AppendLine();
            sb.AppendLine("@Injectable({ providedIn: 'root' })");
            sb.AppendLine($"export class {ir.ComponentName}Service {{");
            sb.AppendLine("  constructor(private http: HttpClient) {}");
            sb.AppendLine();

            foreach (var a in ir.Actions)
            {
                sb.AppendLine($"  {a.Name}(data?: any) {{");
                sb.AppendLine($"    return this.http.{a.Http.ToLower()}('{ir.ApiRoute}', data);");
                sb.AppendLine("  }");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            File.WriteAllText(
                Path.Combine(dir, $"{ir.ComponentName.ToLower()}.service.ts"),
                sb.ToString()
            );
        }

        private void GenerateComponent(AngularIR ir, string dir)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import { Component } from '@angular/core';");
            sb.AppendLine($"import {{ {ir.ComponentName}Service }} from './{ir.ComponentName.ToLower()}.service';");
            sb.AppendLine();
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: 'app-{ir.ComponentName.ToLower()}',");
            sb.AppendLine($"  templateUrl: './{ir.ComponentName.ToLower()}.component.html'");
            sb.AppendLine("})");
            sb.AppendLine($"export class {ir.ComponentName}Component {{");
            sb.AppendLine($"  constructor(private service: {ir.ComponentName}Service) {{}}");
            sb.AppendLine();

            foreach (var a in ir.Actions)
            {
                sb.AppendLine($"  {a.Name}() {{");
                sb.AppendLine($"    this.service.{a.Name}().subscribe();");
                sb.AppendLine("  }");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            File.WriteAllText(
                Path.Combine(dir, $"{ir.ComponentName.ToLower()}.component.ts"),
                sb.ToString()
            );
        }

        private void GenerateTemplate(AngularIR ir, string dir)
        {
            var sb = new StringBuilder();

            foreach (var a in ir.Actions)
            {
                sb.AppendLine(
                    $"<button (click)=\"{a.Name}()\">{a.Name}</button>"
                );
            }

            File.WriteAllText(
                Path.Combine(dir, $"{ir.ComponentName.ToLower()}.component.html"),
                sb.ToString()
            );
        }
    }
}
