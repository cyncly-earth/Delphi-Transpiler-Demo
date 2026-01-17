using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.AstNodes
{
    public class ProcedureDeclNode : AstNode
    {
        public string Name { get; set; }
        public List<string> Params { get; set; } = new();
        public List<string> Body { get; set; } = new();
    }
}
