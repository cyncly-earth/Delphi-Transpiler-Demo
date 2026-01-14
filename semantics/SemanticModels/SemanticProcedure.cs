using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.SemanticModels
{
    public class SemanticProcedure
    {
        public required string Symbol { get; set; }

        // Parameter name → resolved type
        public Dictionary<string, string> Parameters { get; set; } = new();

        // Side effects (writes, reads, calls, etc.)
        public List<string> Effects { get; set; } = new();
    }
}
