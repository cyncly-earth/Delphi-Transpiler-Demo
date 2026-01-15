using System.Text;

namespace DelphiToCsConverter
{
    public class Generators
    {
        public string GenerateBackend(SemanticSolution ir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// --- BACKEND OUTPUT (C# API & DTOs) ---");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine("namespace HotelSystem.Backend");
            sb.AppendLine("{");

            // DTOs
            foreach (var ent in ir.Entities)
            {
                sb.AppendLine($"    public class {ent.Name}Dto");
                sb.AppendLine("    {");
                foreach (var p in ent.Properties)
                {
                    if (p.IsKey) sb.AppendLine("        [Key]");
                    sb.AppendLine($"        public {p.Type} {p.Name} {{ get; set; }}");
                }
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Services
            foreach (var svc in ir.Services)
            {
                sb.AppendLine($"    public interface I{svc.Name}");
                sb.AppendLine("    {");
                foreach (var m in svc.Methods)
                {
                    sb.AppendLine($"        Task {m.Name}Async({m.Signature});");
                }
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string GenerateFrontend(SemanticSolution ir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// --- FRONTEND OUTPUT (C# Logic Wrapper for Angular) ---");
            sb.AppendLine("// Note: This C# code contains the logic needed for Angular.");
            sb.AppendLine("// Developers can copy logic blocks or Transpile this file to TS.");
            sb.AppendLine();
            sb.AppendLine("namespace HotelSystem.Frontend");
            sb.AppendLine("{");

            // ViewModels (camelCase simulation for Angular)
            foreach (var ent in ir.Entities)
            {
                sb.AppendLine($"    public class {ent.Name}Model");
                sb.AppendLine("    {");
                foreach (var p in ent.Properties)
                {
                    // Convert PascalCase to camelCase for FE naming convention
                    string camel = char.ToLower(p.Name[0]) + p.Name.Substring(1);
                    sb.AppendLine($"        public {p.Type} {camel} {{ get; set; }}");
                }
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Logic Helpers
            sb.AppendLine("    public static class LogicHelpers");
            sb.AppendLine("    {");
            foreach (var logic in ir.LogicBlocks)
            {
                sb.AppendLine($"        // Logic from {logic.Name}");
                sb.AppendLine($"        public static void {logic.Name}(/* inferred params */)");
                sb.AppendLine("        {");
                foreach (var line in logic.Lines)
                {
                    sb.AppendLine($"            {line}");
                }
                sb.AppendLine("        }");
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}