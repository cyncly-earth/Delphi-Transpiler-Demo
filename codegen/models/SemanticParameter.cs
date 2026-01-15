namespace DelphiTranspiler.Semantics.SemanticModels
{
    public sealed class SemanticParameter
    {
        public string Name { get; set; } = string.Empty;
 
        /// <summary>
        /// Semantic type of the parameter (Named, Array, etc.)
        /// </summary>
        public SemanticType Type { get; set; } = SemanticType.Unresolved;
    }
}