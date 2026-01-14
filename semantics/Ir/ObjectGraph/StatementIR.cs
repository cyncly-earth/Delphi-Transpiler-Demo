using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.IR.ObjectGraph
{
    // -------------------------------
    // CALL
    // -------------------------------
    public class CallIR : IrNode
    {
        public string Target { get; set; } = "";
        public List<string> Arguments { get; } = new();
    }

    // -------------------------------
    // LOOP
    // -------------------------------
    public class LoopIR : IrNode
    {
        public string Iterator { get; set; } = "";
        public List<IrNode> Body { get; } = new();
    }

    // -------------------------------
    // RETURN
    // -------------------------------
    public class ReturnIR : IrNode
    {
    }
}
