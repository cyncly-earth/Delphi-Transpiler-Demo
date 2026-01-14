using System.Text.Json.Serialization;

namespace DelphiTranspiler.Semantics.IR.ObjectGraph
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
    [JsonDerivedType(typeof(CallIR), "call")]
    [JsonDerivedType(typeof(LoopIR), "loop")]
    [JsonDerivedType(typeof(ReturnIR), "return")]
    public abstract class IrNode
    {
    }
}
