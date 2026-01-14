using Transpiler.AST;
using System.Linq;

// Listener that builds an AstUnit for CalendarView by walking the ANTLR parse tree
public class CalendarViewAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "CalendarView" };

    public override void ExitProcDecl(DelphiParser.ProcDeclContext context)
    {
        var heading = context.procDeclHeading();
        if (heading == null) return;
        var ident = heading.ident();
        if (ident == null) return;
        var name = ident.GetText();
        var parameters = heading.formalParameterSection()?.GetText() ?? string.Empty;
        var returnType = heading.typeDecl()?.GetText() ?? string.Empty;
        var bodyCtx = context.procBody();
        var bodyText = bodyCtx?.GetText() ?? string.Empty;
        var hasBody = bodyCtx != null;

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
