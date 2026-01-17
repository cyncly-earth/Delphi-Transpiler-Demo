using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

// Namespaces
using Transpiler.AST;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics; 

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// =========================================================
// 1. PATH CONFIGURATION
// =========================================================
// This logic finds the 'run' folder regardless of where you run 'dotnet run'
string rootDir = Directory.GetCurrentDirectory();
while (!Directory.Exists(Path.Combine(rootDir, "run")) && Directory.GetParent(rootDir) != null)
{
    rootDir = Directory.GetParent(rootDir)!.FullName;
}

string inputDir     = Path.Combine(rootDir, "run", "input");
string astOutputDir = Path.Combine(rootDir, "output");       
string irOutputDir  = Path.Combine(rootDir, "run", "ir");     
string angularDir   = Path.Combine(rootDir, "run", "angular");

// =========================================================
// 2. PIPELINE LOGIC
// =========================================================
void RunPipeline()
{
    Console.WriteLine("=============================================");
    Console.WriteLine("   🚀 STARTING PIPELINE                      ");
    Console.WriteLine("=============================================");

    // --- POD 1: AST (Parsing) ---
    Console.WriteLine($"[Pod 1] Parsing .pas files...");
    var astProcessor = new AstProcessor(); 
    // We pass the paths explicitly to ensure it finds them
    astProcessor.Run(inputDir, astOutputDir); 

    // --- POD 2: SEMANTICS ---
    Console.WriteLine($"[Pod 2] Enriching AST...");
    if (!Directory.Exists(irOutputDir)) Directory.CreateDirectory(irOutputDir);

    // 1. Load the JSON ASTs we just created
    var loadedUnits = new List<AstUnit>();
    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
    foreach (var file in Directory.GetFiles(astOutputDir, "*.json"))
    {
        string content = File.ReadAllText(file);
        // Deserialize using Transpiler.Semantics.AstUnit
        var unit = JsonSerializer.Deserialize<AstUnit>(content, jsonOptions);
        if (unit != null) loadedUnits.Add(unit);
    }

    Console.WriteLine($"[Pod 2] Loaded {loadedUnits.Count} units.");

    // 2. Run Semantic Enrichment
    var enricher = new SemanticEnrichmentPrototype();
    var runner = new SemanticEnrichmentRunner(enricher);

    // This generates the 3 Semantic JSONs
    var (uiJson, entityJson, backendJson) = runner.ProcessFeature(loadedUnits);

    // 3. Save to Disk
    File.WriteAllText(Path.Combine(irOutputDir, "ui.json"), uiJson);
    File.WriteAllText(Path.Combine(irOutputDir, "entity.json"), entityJson);
    File.WriteAllText(Path.Combine(irOutputDir, "backend.json"), backendJson);

    // --- POD 3: ANGULAR ---
    Console.WriteLine($"[Pod 3] Generating Angular...");
    if (!Directory.Exists(angularDir)) Directory.CreateDirectory(angularDir);

    var angularGen = new AngularGenerator();
    angularGen.Generate(uiJson, angularDir);

    Console.WriteLine("=============================================");
    Console.WriteLine("   ✅ PIPELINE COMPLETE");
    Console.WriteLine("=============================================");
}

try { RunPipeline(); } 
catch (Exception ex) { 
    Console.WriteLine($"ERROR: {ex.Message}"); 
    Console.WriteLine(ex.StackTrace);
}

// =========================================================
// 3. WEB DASHBOARD
// =========================================================
app.MapGet("/", async (HttpContext context) =>
{
    string uiJson = ReadSafe(Path.Combine(irOutputDir, "ui.json"));
    string htmlComp = ReadSafe(Path.Combine(angularDir, "add-person.component.html"));
    string tsComp = ReadSafe(Path.Combine(angularDir, "add-person.component.ts"));

    string html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Pipeline Dashboard</title>
        <style>
            body {{ font-family: sans-serif; background: #222; color: #eee; height: 100vh; display: flex; flex-direction: column; margin:0; }}
            header {{ background: #007acc; padding: 10px; text-align: center; }}
            .container {{ display: flex; flex: 1; overflow: hidden; }}
            .col {{ flex: 1; padding: 10px; border-right: 1px solid #444; display: flex; flex-direction: column; }}
            pre {{ background: #111; flex: 1; overflow: auto; padding: 10px; color: #9cdcfe; }}
        </style>
    </head>
    <body>
        <header>Delphi -> Semantic -> Angular</header>
        <div class='container'>
            <div class='col'>
                <h3>1. Semantic UI Model</h3>
                <pre>{System.Net.WebUtility.HtmlEncode(uiJson)}</pre>
            </div>
            <div class='col'>
                <h3>2. Angular Component</h3>
                <pre>{System.Net.WebUtility.HtmlEncode(tsComp)}</pre>
            </div>
            <div class='col'>
                <h3>3. Angular HTML</h3>
                <pre>{System.Net.WebUtility.HtmlEncode(htmlComp)}</pre>
                <div style='background:white; color:black; padding:10px;'>{htmlComp}</div>
            </div>
        </div>
    </body>
    </html>";

    await context.Response.WriteAsync(html);
});

string ReadSafe(string path) => File.Exists(path) ? File.ReadAllText(path) : "Not found";

app.Run();