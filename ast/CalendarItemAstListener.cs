using Transpiler.AST;

// Listener that builds an AstUnit for classCalendarItem by walking the ANTLR parse tree
public class CalendarItemAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "classCalendarItem" };

    // Capture type declarations (e.g., "TCalendarItem = class ...")
    public override void ExitTypeDeclaration(DelphiParser.TypeDeclarationContext context)
    {
        var typeNameCtx = context.genericTypeIdent();
        var typeDeclCtx = context.typeDecl();
        if (typeNameCtx != null && typeDeclCtx != null)
        {
            var typeName = typeNameCtx.GetText();
            // crude check: if the right side contains 'class', treat it as a class declaration
            if (typeDeclCtx.GetText().IndexOf("class", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
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

                Unit.Classes.Add(new AstClass { Name = typeName, Span = span });
            }
        }
    }
}
