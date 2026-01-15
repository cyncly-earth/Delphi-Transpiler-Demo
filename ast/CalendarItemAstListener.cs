using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Linq;
using System.Collections.Generic;
using DelphiGrammar;

// Listener that builds an AstUnit for classCalendarItem by walking the ANTLR parse tree
public class CalendarItemAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "classCalendarItem" };
    private bool _inInterface = false;
    private bool _inImplementation = false;
    private AstClass? _currentClass = null;
    private string _currentVisibility = "public";
    private AstProcedure? _currentProcedure = null;
    private Stack<List<AstStatement>> _statementStack = new Stack<List<AstStatement>>();

    public override void EnterUnitInterface(DelphiParser.UnitInterfaceContext context)
    {
        _inInterface = true;
        _inImplementation = false;
    }

    public override void EnterUnitImplementation(DelphiParser.UnitImplementationContext context)
    {
        _inInterface = false;
        _inImplementation = true;
    }

    // Capture uses clauses
    public override void ExitUsesClause(DelphiParser.UsesClauseContext context)
    {
        var uses = context.namespaceNameList();
        if (uses == null) return;

        var unitNames = uses.namespaceName().Select(n => n.GetText()).ToList();
        
        if (_inInterface)
        {
            Unit.InterfaceSection.Uses.AddRange(unitNames);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Uses.AddRange(unitNames);
        }
    }

    // Capture type declarations (e.g., "TCalendarItem = class ...")
    public override void EnterTypeDeclaration(DelphiParser.TypeDeclarationContext context)
    {
        var typeNameCtx = context.genericTypeIdent();
        var typeDeclCtx = context.typeDecl();
        if (typeNameCtx == null || typeDeclCtx == null) return;

        var typeName = typeNameCtx.GetText();
        
        // Check if it's a class declaration
        var strucType = typeDeclCtx.strucType();
        if (strucType?.strucTypePart()?.classDecl()?.classTypeDecl() != null)
        {
            var span = GetSpan(context);
            
            _currentClass = new AstClass 
            { 
                Name = typeName, 
                ClassType = "class", 
                Span = span 
            };
            _currentVisibility = "public"; // Default visibility
        }
    }

    public override void ExitTypeDeclaration(DelphiParser.TypeDeclarationContext context)
    {
        if (_currentClass != null)
        {
            if (_inInterface)
            {
                Unit.InterfaceSection.Classes.Add(_currentClass);
            }
            else if (_inImplementation)
            {
                Unit.ImplementationSection.Classes.Add(_currentClass);
            }
            _currentClass = null;
        }
    }

    // Capture visibility changes
    public override void EnterVisibility(DelphiParser.VisibilityContext context)
    {
        var text = context.GetText().ToLower();
        if (text.Contains("private"))
            _currentVisibility = "private";
        else if (text.Contains("protected"))
            _currentVisibility = "protected";
        else if (text.Contains("public"))
            _currentVisibility = "public";
        else if (text.Contains("published"))
            _currentVisibility = "published";
    }

    // Capture class fields
    public override void ExitClassField(DelphiParser.ClassFieldContext context)
    {
        if (_currentClass == null) return;

        var identList = context.identList();
        if (identList == null) return;

        var field = new AstField
        {
            Names = identList.ident().Select(i => i.GetText()).ToList(),
            TypeName = context.typeDecl()?.GetText() ?? string.Empty,
            Visibility = _currentVisibility,
            Span = GetSpan(context)
        };

        _currentClass.Fields.Add(field);
    }

    // Capture class properties
    public override void ExitClassProperty(DelphiParser.ClassPropertyContext context)
    {
        if (_currentClass == null) return;

        var property = new AstProperty
        {
            Name = context.ident()?.GetText() ?? string.Empty,
            TypeName = context.genericTypeIdent()?.GetText() ?? string.Empty,
            Visibility = _currentVisibility,
            Span = GetSpan(context)
        };

        // Extract read and write specifiers
        foreach (var spec in context.classPropertySpecifier())
        {
            var readWrite = spec.classPropertyReadWrite();
            if (readWrite != null)
            {
                var text = readWrite.GetText();
                if (text.StartsWith("read"))
                {
                    property.ReadSpecifier = readWrite.qualifiedIdent()?.GetText() ?? string.Empty;
                }
                else if (text.StartsWith("write"))
                {
                    property.WriteSpecifier = readWrite.qualifiedIdent()?.GetText() ?? string.Empty;
                }
            }
        }

        _currentClass.Properties.Add(property);
    }

    // Capture class methods (constructor, function declarations in interface)
    public override void ExitClassMethod(DelphiParser.ClassMethodContext context)
    {
        if (_currentClass == null) return;

        var methodKey = context.methodKey();
        var ident = context.ident();
        if (ident == null) return;

        var procedureType = "procedure";
        if (methodKey != null)
        {
            var keyText = methodKey.GetText().ToLower();
            if (keyText.Contains("constructor"))
                procedureType = "constructor";
            else if (keyText.Contains("destructor"))
                procedureType = "destructor";
        }
        else if (context.GetText().StartsWith("function", System.StringComparison.OrdinalIgnoreCase))
        {
            procedureType = "function";
        }

        var method = new AstProcedure
        {
            Name = ident.GetText(),
            ProcedureType = procedureType,
            Parameters = ParseParameters(context.formalParameterSection()),
            ReturnType = context.typeDecl()?.GetText() ?? string.Empty,
            Visibility = _currentVisibility,
            Span = GetSpan(context)
        };

        _currentClass.Methods.Add(method);
    }

    // Capture method implementations (in implementation section)
    public override void EnterMethodDecl(DelphiParser.MethodDeclContext context)
    {
        var heading = context.methodDeclHeading();
        if (heading == null) return;

        var methodName = heading.methodName();
        if (methodName == null) return;

        var methodKey = heading.methodKey();
        var procedureType = "function";
        
        if (methodKey != null)
        {
            var keyText = methodKey.GetText().ToLower();
            if (keyText.Contains("constructor"))
                procedureType = "constructor";
            else if (keyText.Contains("destructor"))
                procedureType = "destructor";
            else if (keyText.Contains("procedure"))
                procedureType = "procedure";
        }

        var identParts = methodName.ident();
        var name = identParts.Length > 0 ? identParts[identParts.Length - 1].GetText() : string.Empty;

        _currentProcedure = new AstProcedure
        {
            Name = name,
            ProcedureType = procedureType,
            Parameters = ParseParameters(heading.formalParameterSection()),
            ReturnType = heading.typeDecl()?.GetText() ?? string.Empty,
            Span = GetSpan(context)
        };
        
        // Initialize statement stack for capturing method body
        _statementStack.Clear();
        
        // Pre-push a list for the method body statements
        // This ensures the stack is ready when compound statement is encountered
        _statementStack.Push(new List<AstStatement>());
        System.Console.WriteLine($"[DEBUG] EnterMethodDecl: {name}, pre-pushed stack level");
    }

    public override void ExitMethodDecl(DelphiParser.MethodDeclContext context)
    {
        if (_currentProcedure == null) return;

        // Pop the method body statements from stack
        if (_statementStack.Count > 0)
        {
            var body = _statementStack.Pop();
            System.Console.WriteLine($"[DEBUG] ExitMethodDecl: {_currentProcedure.Name}, popped {body.Count} statements");
            _currentProcedure.Body = body;
        }

        if (_inImplementation)
        {
            Unit.ImplementationSection.Procedures.Add(_currentProcedure);
        }

        _currentProcedure = null;
        _statementStack.Clear();
    }

    // Capture compound statements
    public override void EnterCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var statements = _statementStack.Pop();
        System.Console.WriteLine($"[DEBUG] ExitCompoundStatement: Popped {statements.Count} statements, stack now has {_statementStack.Count} levels");
        
        // If stack count is 1, we're at the method body level - add statements directly without wrapping
        if (_statementStack.Count == 1)
        {
            System.Console.WriteLine($"[DEBUG] Adding {statements.Count} statements directly to method body (no wrapping)");
            foreach (var stmt in statements)
            {
                _statementStack.Peek().Add(stmt);
            }
        }
        // If stack count > 1, we're nested - wrap in compound statement
        else if (_statementStack.Count > 1)
        {
            var compound = new AstCompoundStatement { Statements = statements, Span = GetSpan(context) };
            _statementStack.Peek().Add(compound);
        }
    }

    // Capture simple statements (assignments, calls)
    public override void ExitSimpleStatement(DelphiParser.SimpleStatementContext context)
    {
        if (_statementStack.Count == 0)
        {
            System.Console.WriteLine($"[DEBUG] ExitSimpleStatement: Stack is empty, skipping");
            return;
        }

        var text = context.GetText();
        System.Console.WriteLine($"[DEBUG] ExitSimpleStatement: text='{text}'");
        
        if (text.Contains(":="))
        {
            var parts = text.Split(new[] { ":=" }, 2, System.StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var assignment = new AstAssignment
                {
                    Target = parts[0],
                    Value = parts[1],
                    Span = GetSpan(context)
                };
                System.Console.WriteLine($"[DEBUG] Adding assignment: {parts[0]} := {parts[1]}");
                _statementStack.Peek().Add(assignment);
                return;
            }
        }

        var designator = context.designator();
        if (designator != null)
        {
            var call = new AstProcedureCall
            {
                ProcedureName = ExtractProcedureName(designator),
                Arguments = ExtractArguments(designator),
                Span = GetSpan(context)
            };
            _statementStack.Peek().Add(call);
        }
    }

    // Helper methods
    private List<AstParameter> ParseParameters(DelphiParser.FormalParameterSectionContext? context)
    {
        var parameters = new List<AstParameter>();
        if (context == null) return parameters;

        var paramList = context.formalParameterList();
        if (paramList == null) return parameters;

        foreach (var param in paramList.formalParameter())
        {
            var astParam = new AstParameter
            {
                Names = param.identListFlat()?.ident()?.Select(i => i.GetText()).ToList() ?? new List<string>(),
                TypeName = param.typeDecl()?.GetText() ?? string.Empty,
                ParameterType = param.parmType()?.GetText() ?? string.Empty,
                DefaultValue = param.expression()?.GetText() ?? string.Empty
            };
            parameters.Add(astParam);
        }

        return parameters;
    }

    private string ExtractProcedureName(DelphiParser.DesignatorContext designator)
    {
        var qualified = designator.namespacedQualifiedIdent();
        if (qualified != null)
        {
            return qualified.GetText();
        }
        var typeId = designator.typeId();
        if (typeId != null)
        {
            return typeId.GetText();
        }
        return designator.GetText().Split('(')[0];
    }

    private List<string> ExtractArguments(DelphiParser.DesignatorContext designator)
    {
        var arguments = new List<string>();
        foreach (var item in designator.designatorItem())
        {
            var exprList = item.expressionList();
            if (exprList != null)
            {
                foreach (var expr in exprList.expression())
                {
                    arguments.Add(expr.GetText());
                }
            }
        }
        return arguments;
    }

    private SourceSpan GetSpan(ParserRuleContext context)
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
        return span;
    }
}
