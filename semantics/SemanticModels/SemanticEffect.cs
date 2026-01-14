namespace DelphiTranspiler.Semantics.SemanticModels
{
    public sealed class SemanticEffect
    {
        public EffectKind Kind { get; set; }

        /// <summary>
        /// Logical target of the effect (dataset, service, module)
        /// </summary>
        public string Target { get; set; } = string.Empty;
    }
}
