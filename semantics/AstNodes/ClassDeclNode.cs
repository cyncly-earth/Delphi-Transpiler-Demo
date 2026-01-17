using System.Collections.Generic;

namespace DelphiTranspiler.Semantics.AstNodes
{
    public class ClassDeclNode : AstNode
    {
        public string Name { get; set; }
        public List<string> Fields { get; set; } = new();
        public List<string> Methods { get; set; } = new();
    }
}
