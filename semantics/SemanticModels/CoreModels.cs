using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.SemanticModels
{
    public abstract class SemanticType { }
    public class NamedType : SemanticType { public string QualifiedName { get; set; } = ""; }
    public class ArrayType : SemanticType { public SemanticType ElementType { get; set; } = new UnresolvedType(); }
    public class UnresolvedType : SemanticType { }
    public class ClassType : SemanticType { 
        public string Name { get; set; } = ""; 
        public Dictionary<string, string> Fields { get; } = new(); 
    }

    public class SemanticProcedure {
        public string Name { get; set; } = "";
        public string SourceUnit { get; set; } = "";
        public Dictionary<string, string> Parameters { get; } = new();
        public List<string> Reads { get; } = new();
        public List<string> Writes { get; } = new();
        public List<string> Creates { get; } = new();
        public List<string> Calls { get; } = new();
        public bool IsUiProcedure { get; set; } = false;
    }

    public class BackendIr { public List<BackendProcedure> Procedures { get; set; } = new(); }
    public class BackendProcedure { 
        public string Name { get; set; } = ""; 
        public List<BackendParam> Params { get; set; } = new(); 
        public List<string> Actions { get; set; } = new(); 
    }
    public class BackendParam { public string Name { get; set; } = ""; public object Type { get; set; } = ""; }

    public sealed class EntityModel { public List<EntityDefinition> Entities { get; } = new(); }
    public sealed class EntityDefinition { public string Name { get; set; } = ""; public List<EntityField> Fields { get; } = new(); }
    public sealed class EntityField { public string Name { get; set; } = ""; public string Type { get; set; } = ""; }
}