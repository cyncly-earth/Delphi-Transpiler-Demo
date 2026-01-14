using System.Text;

namespace CodeGen
{
    public class AngularGenerator
    {
        private readonly string _outputDir = "output/angular";

        public void Generate(SemanticProcedure procedure)
        {
            Directory.CreateDirectory(_outputDir);

            GenerateComponent(procedure);
            GenerateHtml(procedure);
            GenerateService(procedure);
        }

        // ---------------- COMPONENT ----------------

        private void GenerateComponent(SemanticProcedure proc)
        {
            var className = $"{proc.Name}Component";
            var selector = $"app-{ToKebab(proc.Name)}";

            var sb = new StringBuilder();

            sb.AppendLine("import { Component } from '@angular/core';");
            sb.AppendLine($"import {{ {proc.Name}Service }} from './{ToKebab(proc.Name)}.service';");
            sb.AppendLine();
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: '{selector}',");
            sb.AppendLine($"  templateUrl: './{ToKebab(proc.Name)}.component.html'");
            sb.AppendLine("})");
            sb.AppendLine($"export class {className} {{");

            // Models
            foreach (var p in proc.Parameters)
            {
                if (p.Type is ArrayType)
                    sb.AppendLine($"  {ToCamel(p.Name)}: any[] = [];");
                else
                    sb.AppendLine($"  {ToCamel(p.Name)}: any = {{}};");
            }

            sb.AppendLine();
            sb.AppendLine($"  constructor(private service: {proc.Name}Service) {{ }}");
            sb.AppendLine();

            // Save method
            sb.AppendLine($"  save() {{");
            sb.AppendLine("    const payload = {");

            foreach (var p in proc.Parameters)
                sb.AppendLine($"      {ToCamel(p.Name)}: this.{ToCamel(p.Name)},");

            sb.AppendLine("    };");
            sb.AppendLine();
            sb.AppendLine($"    this.service.{ToCamel(proc.Name)}(payload).subscribe();");
            sb.AppendLine("  }");

            sb.AppendLine("}");

            File.WriteAllText(
                Path.Combine(_outputDir, $"{ToKebab(proc.Name)}.component.ts"),
                sb.ToString()
            );
        }

        // ---------------- HTML ----------------

        private void GenerateHtml(SemanticProcedure proc)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<h2>{proc.Name}</h2>");
            sb.AppendLine();

            foreach (var p in proc.Parameters)
            {
                if (p.Type is ArrayType)
                {
                    sb.AppendLine($"<div *ngFor=\"let item of {ToCamel(p.Name)}\">");
                    sb.AppendLine("  <input placeholder=\"value\" />");
                    sb.AppendLine("</div>");
                    sb.AppendLine($"<button (click)=\"{ToCamel(p.Name)}.push({{}})\">Add {p.Name}</button>");
                }
                else
                {
                    sb.AppendLine($"<input placeholder=\"{p.Name}\" [(ngModel)]=\"{ToCamel(p.Name)}\" />");
                }

                sb.AppendLine();
            }

            sb.AppendLine("<button (click)=\"save()\">Save</button>");

            File.WriteAllText(
                Path.Combine(_outputDir, $"{ToKebab(proc.Name)}.component.html"),
                sb.ToString()
            );
        }

        // ---------------- SERVICE ----------------

        private void GenerateService(SemanticProcedure proc)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import { Injectable } from '@angular/core';");
            sb.AppendLine("import { HttpClient } from '@angular/common/http';");
            sb.AppendLine();
            sb.AppendLine("@Injectable({ providedIn: 'root' })");
            sb.AppendLine($"export class {proc.Name}Service {{");
            sb.AppendLine();
            sb.AppendLine("  constructor(private http: HttpClient) { }");
            sb.AppendLine();
            sb.AppendLine($"  {ToCamel(proc.Name)}(data: any) {{");

            // WRITE effect → POST
            var isWrite = proc.Effects.Any(e => e.Kind == EffectKind.Write);
            var httpCall = isWrite ? "post" : "get";

            sb.AppendLine($"    return this.http.{httpCall}('/api/{proc.Name}', data);");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            File.WriteAllText(
                Path.Combine(_outputDir, $"{ToKebab(proc.Name)}.service.ts"),
                sb.ToString()
            );
        }

        // ---------------- HELPERS ----------------

        private string ToKebab(string value) =>
            string.Concat(value.Select((c, i) =>
                char.IsUpper(c) && i > 0 ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));

        private string ToCamel(string value) =>
            char.ToLower(value[0]) + value.Substring(1);
    }
}
