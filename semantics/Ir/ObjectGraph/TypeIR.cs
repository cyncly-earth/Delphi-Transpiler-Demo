using System.Text.Json.Serialization;

namespace DelphiTranspiler.Semantics.IR.ObjectGraph
{
    // -------------------------------------------------
    // Base type (polymorphic)
    // -------------------------------------------------
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(NamedTypeIR), "named")]
    [JsonDerivedType(typeof(ArrayTypeIR), "array")]
    [JsonDerivedType(typeof(ClassTypeIR), "class")]
    public abstract class TypeIR
    {
    }

    // -------------------------------------------------
    // Named type (e.g. Person, Contact)
    // -------------------------------------------------
    public class NamedTypeIR : TypeIR
    {
        public required string Name { get; set; }
    }

    // -------------------------------------------------
    // Array type (e.g. Array<Contact>)
    // -------------------------------------------------
    public class ArrayTypeIR : TypeIR
    {
        public required TypeIR ElementType { get; set; }
    }

    // -------------------------------------------------
    // Class type (e.g. TPerson)
    // -------------------------------------------------
    public class ClassTypeIR : TypeIR
    {
        public required string Name { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new();
    }
}
