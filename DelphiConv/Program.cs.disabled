using System;
using System.IO;
using System.Text.Json;


namespace DelphiToCsConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- LOADING AST FILES ---");
            
            // 1. Load Files
            var itemAst = Load("ast_input/calendarItem.json");
            var viewAst = Load("ast_input/calendarView.json");
            var ctrlAst = Load("ast_input/calendarController.json");

            if(itemAst == null) { Console.WriteLine("STOP: Verify ast_input/calendarItem.json exists."); return; }

           Console.WriteLine("Calling Converter.Process with:");
//Console.WriteLine("itemAst = " + JsonSerializer.Serialize(itemAst));
//Console.WriteLine("viewAst = " + JsonSerializer.Serialize(viewAst));
//Console.WriteLine("ctrlAst = " + JsonSerializer.Serialize(ctrlAst));

            // 2. Convert
            Console.WriteLine("--- CONVERTING TO SEMANTIC IR ---");
            var converter = new Converter();
            var ir = converter.Process(itemAst, viewAst, ctrlAst);

            // 3. Generate Files
            var gen = new Generators();
            
            Console.WriteLine("--- WRITING BACKEND FILE ---");
            File.WriteAllText("Backend_Output.cs", gen.GenerateBackend(ir));

            Console.WriteLine("--- WRITING FRONTEND FILE ---");
            File.WriteAllText("Frontend_Output.cs", gen.GenerateFrontend(ir));

            Console.WriteLine("\nSUCCESS! Check 'Backend_Output.cs' and 'Frontend_Output.cs'");
        }

        static DelphiUnit Load(string path)
        {
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            // Relaxed JSON parsing
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip };
            try { return JsonSerializer.Deserialize<DelphiUnit>(json, opts); }
            catch (Exception ex) { Console.WriteLine($"Error parsing {path}: {ex.Message}"); return null; }
        }
    }
}
