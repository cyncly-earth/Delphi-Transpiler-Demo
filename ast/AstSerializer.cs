using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transpiler.AST;

public static class AstSerializer
{
    private static readonly JsonSerializerOptions Options =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = true
        };

    public static void Save(AstUnit unit, string path)
    {
        var json = JsonSerializer.Serialize(unit, Options);
        System.Console.WriteLine($"[AstSerializer] Saving {path}");
        File.WriteAllText(path, json);
    }

    public static AstUnit Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AstUnit>(json)!
            ?? throw new InvalidOperationException("Invalid AST file");
    }
}
