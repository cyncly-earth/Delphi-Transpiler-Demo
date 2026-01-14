namespace DelphiTranspiler.Semantics.IR.ObjectGraph
{
    public class ParameterIR
    {
        public string Name { get; set; } = "";
        public required TypeIR Type { get; set; }
    }
}
