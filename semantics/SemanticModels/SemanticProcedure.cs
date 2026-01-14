using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.SemanticModels
{
    public sealed class SemanticProcedure
    {
        public string Name { get; set; } = string.Empty;
        public List<SemanticParameter> Parameters { get; set; } = new();
        public List<SemanticEffect> Effects { get; set; } = new();
    }
}

