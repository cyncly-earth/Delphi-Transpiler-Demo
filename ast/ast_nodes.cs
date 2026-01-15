using System.Collections.Generic;

namespace DelphiTranspiler.Ast;

public class TranspilerOutput
{
    public List<MethodNode> ControllerMethods { get; set; } = new();
    public List<string> RequiredModels { get; set; } = new();
    public List<DataMapping> ViewMappings { get; set; } = new();
}

// public class MethodNode
// {
//     public string Name { get; set; }
//     public List<ParamNode> Parameters { get; set; } = new();
//     public string BodyRaw { get; set; }
// }
public class MethodNode
{
    // Adding 'required' ensures these must be set when the object is created
    public required string Name { get; set; } 
    public List<ParamNode> Parameters { get; set; } = new();
    public string BodyRaw { get; set; } = string.Empty; // Or initialize to empty
}

public record ParamNode(string Name, string Type);
public record DataMapping(string FieldName, string SourceProperty);