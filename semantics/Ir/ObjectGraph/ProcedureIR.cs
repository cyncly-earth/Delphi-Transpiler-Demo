using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.IR.ObjectGraph
{
    public class ProcedureIR : IrNode
    {
        public string Name { get; set; } = "";
        public List<ParameterIR> Parameters { get; } = new();
        public List<IrNode> Body { get; } = new();
    }
}
