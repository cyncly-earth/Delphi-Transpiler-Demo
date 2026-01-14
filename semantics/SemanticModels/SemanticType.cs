using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.SemanticModels
{
    public class SemanticType
    {
        public string Name { get; set; }

        // Field name → resolved type
        public Dictionary<string, string> Fields { get; set; } = new();
    }
}
