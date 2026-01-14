using System.Collections.Generic;

namespace Transpiler.AST
{
    public class AstUnit
    {
        public string Name { get; set; } = string.Empty;
        public List<AstClass> Classes { get; set; } = new();
        public List<AstProcedure> Procedures { get; set; } = new();
    }

    public class AstClass
    {
        public string Name { get; set; } = string.Empty;
        public List<AstProcedure> Methods { get; set; } = new();
        public SourceSpan Span { get; set; } = new();
    }

    public class AstProcedure
    {
        public string Name { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty; // raw parameter text
        public string ReturnType { get; set; } = string.Empty; // raw return type text
        public string Body { get; set; } = string.Empty; // raw body text (statements)
        public bool HasBody { get; set; } = false;
        public SourceSpan Span { get; set; } = new();
    }

    public class SourceSpan
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
