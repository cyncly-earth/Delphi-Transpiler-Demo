namespace DelphiTranspiler.Semantics.SemanticModels
{
    public abstract class SemanticType
    {
        public static readonly SemanticType Unresolved = new UnresolvedType();
    }

    public sealed class UnresolvedType : SemanticType
    {
    }

    public sealed class NamedType : SemanticType
    {
        public string QualifiedName { get; set; } = string.Empty;
    }

    public sealed class ArrayType : SemanticType
    {
        public SemanticType ElementType { get; set; } = SemanticType.Unresolved;
    }
}
