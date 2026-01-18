using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc; // For API simulation

// Namespaces
using Transpiler.AST;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics; 
using DelphiTranspiler.CodeGen.Models;
using DelphiTranspiler.CodeGen.DotNet; // Pod 4

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// --- PATHS ---
string rootDir = Directory.GetCurrentDirectory();
while (!Directory.Exists(Path.Combine(rootDir, "run")) && Directory.GetParent(rootDir) != null)
    rootDir = Directory.GetParent(rootDir)!.FullName;

string inputDir     = Path.Combine(rootDir, "run", "input");
string astOutputDir = Path.Combine(rootDir, "output");       
string irOutputDir  = Path.Combine(rootDir, "run", "ir");     
string angularDir   = Path.Combine(rootDir, "run", "angular");
string dotnetDir    = Path.Combine(rootDir, "run", "dotnet"); // Output for .NET code

// --- PIPELINE ---
try 
{
    Console.WriteLine("🚀 FULL STACK PIPELINE STARTING...");
    
    // 1. AST
    new AstProcessor().Run(inputDir, astOutputDir); 

    // 2. Semantic
    if (!Directory.Exists(irOutputDir)) Directory.CreateDirectory(irOutputDir);
    var loadedUnits = new List<AstUnit>();
    foreach (var file in Directory.GetFiles(astOutputDir, "*.json"))
        loadedUnits.Add(JsonSerializer.Deserialize<AstUnit>(File.ReadAllText(file), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
    
    var runner = new SemanticEnrichmentRunner(new SemanticEnrichmentPrototype());
    var (uiJson, entityJson, backendJson) = runner.ProcessFeature(loadedUnits);
    File.WriteAllText(Path.Combine(irOutputDir, "ui.json"), uiJson);
    File.WriteAllText(Path.Combine(irOutputDir, "entity.json"), entityJson);
    File.WriteAllText(Path.Combine(irOutputDir, "backend.json"), backendJson);

    // 3. Angular
    if (!Directory.Exists(angularDir)) Directory.CreateDirectory(angularDir);
    new AngularGenerator().Generate(uiJson, angularDir);

    // 4. .NET Backend (Pod 4)
    if (!Directory.Exists(dotnetDir)) Directory.CreateDirectory(dotnetDir);
    new DotNetGenerator().Generate(entityJson, backendJson, dotnetDir);

    Console.WriteLine("✅ PIPELINE FINISHED");
}
catch (Exception ex) { Console.WriteLine($"❌ ERROR: {ex.Message}"); }

// --- WORKING API SIMULATION (BACKEND) ---
// This mocks the generated C# Controller so the frontend can actually talk to it
app.MapPost("/api/AddPerson", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"[API Received] {body}");
    return Results.Ok(new { status = "Saved to Database", receivedData = JsonSerializer.Deserialize<object>(body) });
});

// --- DASHBOARD UI ---
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";

    string htmlComp = ReadSafe(Path.Combine(angularDir, "add-person.component.html"));
    string csharpController = ReadSafe(Path.Combine(dotnetDir, "PersonController.cs"));
    string csharpModel = ReadSafe(Path.Combine(dotnetDir, "TPerson.cs"));

    // Prepare Functional Preview
    // We inject a script that simulates Angular's HttpClient hitting our /api/AddPerson
    string script = @"
        <script>
            async function submitForm(event) {
                event.preventDefault();
                const formData = new FormData(event.target);
                const data = Object.fromEntries(formData.entries());
                
                document.getElementById('status').innerText = 'Sending to .NET Backend...';
                
                try {
                    const response = await fetch('/api/AddPerson', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(data)
                    });
                    const result = await response.json();
                    document.getElementById('status').innerHTML = 
                        '<span style=\'color:green\'>✅ ' + result.status + '</span><br>' + 
                        '<pre>' + JSON.stringify(result.receivedData, null, 2) + '</pre>';
                } catch (e) {
                    document.getElementById('status').innerText = 'Error: ' + e;
                }
            }
        </script>";

    string cleanHtml = CleanAngularForPreview(htmlComp); // Uses the updated helper below

    string iframeContent = $@"
        <html>
        <head>
            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
            <style>body {{ padding: 20px; background: white; }} pre {{ background:#f4f4f4; padding:5px; }}</style>
        </head>
        <body>
            {cleanHtml}
            <div style='margin-top: 20px; border-top:1px solid #ccc; padding-top:10px;'>
                <strong>Backend Response:</strong>
                <div id='status' style='font-size: 0.9em; color: #666;'>Waiting for submit...</div>
            </div>
            {script}
        </body>
        </html>";

    string srcDoc = iframeContent.Replace("\"", "&quot;");

    string page = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Full Stack Transpiler</title>
        <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
        <style>
            body {{ background: #e9ecef; height: 100vh; display: flex; flex-direction: column; margin: 0; }}
            .main-content {{ flex: 1; display: flex; padding: 20px; gap: 20px; }}
            .code-panel {{ flex: 1; background: #1e1e1e; color: #d4d4d4; border-radius: 8px; padding: 15px; font-family: monospace; overflow: auto; height: 600px; }}
            .preview-panel {{ flex: 1; background: white; border-radius: 8px; padding: 0; overflow: hidden; display: flex; flex-direction: column; height: 600px; }}
            iframe {{ flex: 1; border: none; width: 100%; height: 100%; }}
        </style>
    </head>
    <body>
        <nav class='navbar navbar-dark bg-dark px-3'>
            <span class='navbar-brand mb-0 h1'>🚀 Delphi -> Angular + .NET Core (Full Working Model)</span>
        </nav>

        <div class='main-content'>
            <!-- LEFT: GENERATED BACKEND CODE -->
            <div style='flex: 1; display: flex; flex-direction: column;'>
                <h4>Generated .NET Backend</h4>
                <div class='code-panel'>
                    <div style='color: #569cd6; font-weight: bold;'>// PersonController.cs</div>
                    <pre>{System.Net.WebUtility.HtmlEncode(csharpController)}</pre>
                    <hr style='border-color: #555;'>
                    <div style='color: #4ec9b0; font-weight: bold;'>// TPerson.cs (Model)</div>
                    <pre>{System.Net.WebUtility.HtmlEncode(csharpModel)}</pre>
                </div>
            </div>

            <!-- RIGHT: WORKING APP -->
            <div style='flex: 1; display: flex; flex-direction: column;'>
                <h4 class='text-success'>Functional App Preview</h4>
                <div class='preview-panel' style='border: 4px solid #28a745;'>
                    <iframe srcdoc=""{srcDoc}""></iframe>
                </div>
            </div>
        </div>
    </body>
    </html>";

    await context.Response.WriteAsync(page);
});

string ReadSafe(string path) => File.Exists(path) ? File.ReadAllText(path) : "File not found";

string CleanAngularForPreview(string angularHtml)
{
    // Convert Angular form to standard HTML form that calls our JS function
    string clean = angularHtml.Replace("(ngSubmit)=\"submit()\"", "onsubmit=\"submitForm(event)\"");
    
    // Remove [(ngModel)] but keep name attribute so FormData works
    clean = Regex.Replace(clean, @"\[\(ngModel\)\]=""[^""]*""", "");
    
    return clean;
}

app.Run();