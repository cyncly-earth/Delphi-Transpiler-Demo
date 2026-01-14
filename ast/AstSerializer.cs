using System.Text.Json;

namespace Transpiler.AST;

public static class AstSerializer
{
    private static readonly JsonSerializerOptions Options =
        new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    public static void Save(AstUnit unit, string path)
    {
        var json = JsonSerializer.Serialize(unit, Options);
        System.Console.WriteLine($"[AstSerializer] Saving {path} -> {json}");
        File.WriteAllText(path, json);
    }

    public static AstUnit Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AstUnit>(json)!
            ?? throw new InvalidOperationException("Invalid AST file");
    }
}
