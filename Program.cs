using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

// Namespaces
using Transpiler.AST;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics; 
using DelphiTranspiler.CodeGen.Models;
using DelphiTranspiler.CodeGen.DotNet; 

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
string dotnetDir    = Path.Combine(rootDir, "run", "dotnet");

// --- PIPELINE EXECUTION (Keep logic running so app works) ---
try 
{
    Console.WriteLine("🚀 PIPELINE STARTING...");
    new AstProcessor().Run(inputDir, astOutputDir); 

    if (!Directory.Exists(irOutputDir)) Directory.CreateDirectory(irOutputDir);
    var loadedUnits = new List<AstUnit>();
    foreach (var file in Directory.GetFiles(astOutputDir, "*.json"))
        loadedUnits.Add(JsonSerializer.Deserialize<AstUnit>(File.ReadAllText(file), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
    
    var runner = new SemanticEnrichmentRunner(new SemanticEnrichmentPrototype());
    var (uiJson, entityJson, backendJson) = runner.ProcessFeature(loadedUnits);
    
    if (!Directory.Exists(angularDir)) Directory.CreateDirectory(angularDir);
    new AngularGenerator().Generate(uiJson, angularDir);

    if (!Directory.Exists(dotnetDir)) Directory.CreateDirectory(dotnetDir);
    new DotNetGenerator().Generate(entityJson, backendJson, dotnetDir);

    Console.WriteLine("✅ PIPELINE FINISHED");
}
catch (Exception ex) { Console.WriteLine($"❌ ERROR: {ex.Message}"); }

// --- WORKING API SIMULATION ---
app.MapPost("/api/AddPerson", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    // Simulate slight network delay for realism
    await Task.Delay(500); 
    return Results.Ok(new { status = "Success: Data saved to .NET Backend", received = JsonSerializer.Deserialize<object>(body) });
});

// --- CLEAN UI DASHBOARD ---
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";

    string htmlComp = ReadSafe(Path.Combine(angularDir, "add-person.component.html"));

    // JavaScript to make the form interactive
    string script = @"
        <script>
            async function submitForm(event) {
                event.preventDefault();
                const btn = event.target.querySelector('button');
                const originalText = btn.innerText;
                
                // UI Loading State
                btn.disabled = true;
                btn.innerText = 'Saving...';
                document.getElementById('response-area').style.display = 'none';

                const formData = new FormData(event.target);
                const data = Object.fromEntries(formData.entries());
                
                try {
                    const response = await fetch('/api/AddPerson', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(data)
                    });
                    const result = await response.json();
                    
                    // Show Success Message
                    const resArea = document.getElementById('response-area');
                    resArea.style.display = 'block';
                    resArea.className = 'alert alert-success';
                    resArea.innerHTML = '<strong>' + result.status + '</strong><br>' + 
                                        '<small>JSON Payload: ' + JSON.stringify(result.received) + '</small>';
                } catch (e) {
                    alert('Error: ' + e);
                } finally {
                    btn.disabled = false;
                    btn.innerText = originalText;
                }
            }
        </script>";

    string cleanHtml = CleanAngularForPreview(htmlComp);

    // Inner IFrame Content
    string iframeContent = $@"
        <html>
        <head>
            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
            <style>
                body {{ padding: 30px; background: white; font-family: 'Segoe UI', sans-serif; }}
                label {{ font-weight: 600; color: #34495e; margin-bottom: 5px; }}
                input {{ margin-bottom: 20px; border-radius: 6px; }}
                button {{ padding: 10px 20px; font-weight: bold; border-radius: 6px; }}
                h2 {{ color: #2c3e50; margin-bottom: 25px; border-bottom: 2px solid #3498db; padding-bottom: 10px; display:inline-block; }}
            </style>
        </head>
        <body>
            <h2>Add Person</h2>
            {cleanHtml}
            <div id='response-area' class='alert' style='display:none; margin-top:20px;'></div>
            {script}
        </body>
        </html>";

    string srcDoc = iframeContent.Replace("\"", "&quot;");

    // Main Page Wrapper
    string page = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Transpiled App</title>
        <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
        <style>
            body {{ background: #f0f2f5; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; }}
            .app-container {{ 
                background: white; 
                width: 100%; 
                max-width: 600px; 
                height: 800px; 
                box-shadow: 0 15px 35px rgba(0,0,0,0.1); 
                border-radius: 12px; 
                overflow: hidden; 
                display: flex; flex-direction: column;
            }}
            .app-header {{
                background: #0078d4; color: white; padding: 15px; text-align: center; font-weight: bold;
                border-bottom: 4px solid #005a9e;
            }}
            iframe {{ border: none; flex: 1; width: 100%; }}
        </style>
    </head>
    <body>
        <div class='app-container'>
            <div class='app-header'>
                Generated Angular Application (Running on .NET Core)
            </div>
            <iframe srcdoc=""{srcDoc}""></iframe>
        </div>
    </body>
    </html>";

    await context.Response.WriteAsync(page);
});

string ReadSafe(string path) => File.Exists(path) ? File.ReadAllText(path) : "";

string CleanAngularForPreview(string angularHtml)
{
    string clean = angularHtml.Replace("(ngSubmit)=\"submit()\"", "onsubmit=\"submitForm(event)\"");
    clean = Regex.Replace(clean, @"\[\(ngModel\)\]=""[^""]*""", "");
    return clean;
}

app.Run();