namespace DelphiTranspiler.CodeGen.Models
{
    public sealed class SemanticParameter
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Semantic type of the parameter (Named, Array, etc.)
        /// </summary>
        public object Type { get; set; } = null!;
    }
}