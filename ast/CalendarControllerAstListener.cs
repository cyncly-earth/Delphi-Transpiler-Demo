using Transpiler.AST;
using Antlr4.Runtime.Tree;
using System.Linq;

// Listener that builds an AstUnit for CalendarController by walking the ANTLR parse tree
public class CalendarControllerAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "CalendarController" };

    // Build a full procedure node when exiting a procDecl (includes heading and optional body)
    public override void ExitProcDecl(DelphiParser.ProcDeclContext context)
    {
        var heading = context.procDeclHeading();
        if (heading == null) return;

        var ident = heading.ident();
        if (ident == null) return;

        var name = ident.GetText();

        // parameters and return type (if any)
        var parameters = heading.formalParameterSection()?.GetText() ?? string.Empty;
        var returnType = heading.typeDecl()?.GetText() ?? string.Empty;

        // body
        var bodyCtx = context.procBody();
        var bodyText = bodyCtx?.GetText() ?? string.Empty;
        var hasBody = bodyCtx != null;

        // source span (use procDecl context tokens)
        var span = new SourceSpan();
        if (context.Start != null)
        {
            span.StartLine = context.Start.Line;
            span.StartColumn = context.Start.Column;
        }
        if (context.Stop != null)
        {
            span.EndLine = context.Stop.Line;
            span.EndColumn = context.Stop.Column;
        }

        if (!Unit.Procedures.Any(p => p.Name == name && p.Span.StartLine == span.StartLine))
        {
            Unit.Procedures.Add(new AstProcedure
            {
                Name = name,
                Parameters = parameters,
                ReturnType = returnType,
                Body = bodyText,
                HasBody = hasBody,
                Span = span
            });
        }
    }
}
