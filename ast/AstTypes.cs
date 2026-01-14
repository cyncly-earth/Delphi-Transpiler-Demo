namespace Transpiler.AST
{
    public class AstUnit
    {
        public string Name { get; set; }
        public List<AstClass> Classes { get; set; } = new();
        public List<AstProcedure> Procedures { get; set; } = new();
    }

    public class AstClass
    {
        public string Name { get; set; }
        public List<AstField> Fields { get; set; } = new();
        public List<AstProcedure> Methods { get; set; } = new();
        public SourceSpan Span { get; set; }
    }

    public class AstField
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public SourceSpan Span { get; set; }
}


    public class AstProcedure
    {
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string ReturnType { get; set; }
        public bool HasBody { get; set; }
        public string Body { get; set; }
        public SourceSpan Span { get; set; }
    }

    public class SourceSpan
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}