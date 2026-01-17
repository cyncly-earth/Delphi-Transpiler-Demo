using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Transpiler.Semantics
{
    public class AstUnit
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("uses")] public List<string> Uses { get; set; } = new();
        [JsonPropertyName("classes")] public List<AstClass> Classes { get; set; } = new();
        [JsonPropertyName("procedures")] public List<AstProcedure> Procedures { get; set; } = new();
        [JsonPropertyName("fields")] public List<AstField> Fields { get; set; } = new();
    }

    public class AstClass { 
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("fields")] public List<AstField> Fields { get; set; } = new();
        [JsonPropertyName("methods")] public List<AstProcedure> Methods { get; set; } = new();
        [JsonPropertyName("span")] public SourceSpan Span { get; set; } = new();
    }
    public class AstField { 
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("span")] public SourceSpan Span { get; set; } = new();
    }
    public class AstProcedure { 
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("kind")] public string Kind { get; set; } = "procedure";
        [JsonPropertyName("parameters")] public string Parameters { get; set; } = string.Empty;
        [JsonPropertyName("returnType")] public string ReturnType { get; set; } = string.Empty;
        [JsonPropertyName("hasBody")] public bool HasBody { get; set; } = false;
        [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
        [JsonPropertyName("span")] public SourceSpan Span { get; set; } = new();
    }
    public class SourceSpan {
        [JsonPropertyName("startLine")] public int StartLine { get; set; }
        [JsonPropertyName("endLine")] public int EndLine { get; set; }
        [JsonPropertyName("startColumn")] public int StartColumn { get; set; }
        [JsonPropertyName("endColumn")] public int EndColumn { get; set; }
    }
}