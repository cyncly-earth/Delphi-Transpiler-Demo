using System.Text;
using System.Text.Json;
using System.IO;
using DelphiTranspiler.Semantics.SemanticModels;

namespace DelphiTranspiler.CodeGen.DotNet
{
    public class DotNetGenerator
    {
        public void Generate(string entityJson, string backendJson, string outputDir)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var entityModel = JsonSerializer.Deserialize<EntityModel>(entityJson, options);
            var backendIr = JsonSerializer.Deserialize<BackendIr>(backendJson, options);

            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            // 1. Generate Models (Person.cs)
            if (entityModel != null)
            {
                foreach (var entity in entityModel.Entities)
                {
                    GenerateEntity(entity, outputDir);
                }
            }

            // 2. Generate Controller (PersonController.cs)
            if (backendIr != null)
            {
                GenerateController(backendIr, outputDir);
            }
        }

        private void GenerateEntity(EntityDefinition entity, string dir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("namespace GeneratedApp.Models");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {entity.Name}");
            sb.AppendLine("    {");
            foreach (var field in entity.Fields)
            {
                // Convert type (e.g., string -> string, int -> int)
                sb.AppendLine($"        public {field.Type} {Capitalize(field.Name)} {{ get; set; }}");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(Path.Combine(dir, $"{entity.Name}.cs"), sb.ToString());
        }

        private void GenerateController(BackendIr ir, string dir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using GeneratedApp.Models;");
            sb.AppendLine();
            sb.AppendLine("namespace GeneratedApp.Controllers");
            sb.AppendLine("{");
            sb.AppendLine("    [ApiController]");
            sb.AppendLine("    [Route(\"api/[controller]\")]");
            sb.AppendLine("    public class PersonController : ControllerBase");
            sb.AppendLine("    {");

            foreach (var proc in ir.Procedures)
            {
                sb.AppendLine($"        [HttpPost(\"{proc.Name}\")]");
                sb.AppendLine($"        public IActionResult {proc.Name}([FromBody] TPerson data)");
                sb.AppendLine("        {");
                sb.AppendLine("            // Logic transpiled from Delphi");
                foreach(var action in proc.Actions) 
                {
                    sb.AppendLine($"            // {action}");
                }
                sb.AppendLine("            return Ok(new { message = \"Success\", data });");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(Path.Combine(dir, "PersonController.cs"), sb.ToString());
        }

        private string Capitalize(string s) => char.ToUpper(s[0]) + s.Substring(1);
    }
}