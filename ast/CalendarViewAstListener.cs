using Transpiler.AST;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System.Linq;
using System.Collections.Generic;
using DelphiGrammar;

// Listener that builds an AstUnit for CalendarView by walking the ANTLR parse tree
public class CalendarViewAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "CalendarView" };
    private bool _inInterface = false;
    private bool _inImplementation = false;
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

    public override void ExitUsesClause(DelphiParser.UsesClauseContext context)
    {
        var namespaceList = context.namespaceNameList();
        if (namespaceList == null) return;

        var uses = namespaceList.namespaceName()
            .Select(ns => ns.GetText())
            .ToList();

        if (_inInterface)
        {
            Unit.InterfaceSection.Uses.AddRange(uses);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Uses.AddRange(uses);
        }
    }

    public override void ExitConstDeclaration(DelphiParser.ConstDeclarationContext context)
    {
        var ident = context.ident();
        if (ident == null) return;

        var constDecl = new AstConstDeclaration
        {
            Name = ident.GetText(),
            TypeName = context.typeDecl()?.GetText() ?? string.Empty,
            Value = context.constExpression()?.GetText() ?? string.Empty,
            Span = GetSpan(context)
        };

        if (_currentProcedure != null)
        {
            _currentProcedure.LocalConstants.Add(constDecl);
        }
        else if (_inInterface)
        {
            Unit.InterfaceSection.Constants.Add(constDecl);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Constants.Add(constDecl);
        }
    }

    public override void ExitVarDeclaration(DelphiParser.VarDeclarationContext context)
    {
        var identList = context.identListFlat();
        if (identList == null) return;

        var varDecl = new AstVarDeclaration
        {
            Names = identList.ident().Select(i => i.GetText()).ToList(),
            TypeName = context.typeDecl()?.GetText() ?? string.Empty,
            InitialValue = context.varValueSpec()?.GetText() ?? string.Empty,
            Span = GetSpan(context)
        };

        if (_currentProcedure != null)
        {
            _currentProcedure.LocalVariables.Add(varDecl);
        }
        else if (_inInterface)
        {
            Unit.InterfaceSection.Variables.Add(varDecl);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Variables.Add(varDecl);
        }
    }

    public override void EnterProcDecl(DelphiParser.ProcDeclContext context)
    {
        var heading = context.procDeclHeading();
        if (heading == null) return;
        var ident = heading.ident();
        if (ident == null) return;
        
        var name = ident.GetText();
        var headingText = heading.GetText();
        var isFunction = headingText.StartsWith("function", System.StringComparison.OrdinalIgnoreCase);
        
        var parameters = ParseParameters(heading.formalParameterSection());

        _currentProcedure = new AstProcedure
        {
            Name = name,
            ProcedureType = isFunction ? "function" : "procedure",
            Parameters = parameters,
            ReturnType = heading.typeDecl()?.GetText() ?? string.Empty,
            IsForwardDeclaration = context.procBody()?.GetText().Contains("forward") ?? false,
            Span = GetSpan(context)
        };

        // Parse directives
        foreach (var directive in context.functionDirective())
        {
            var directiveText = directive.GetText().TrimEnd(';');
            if (!string.IsNullOrEmpty(directiveText))
            {
                _currentProcedure.Directives.Add(directiveText);
            }
        }

        // If not a forward declaration, initialize statement stack for capturing body
        if (!_currentProcedure.IsForwardDeclaration)
        {
            _statementStack.Clear();
            _statementStack.Push(new List<AstStatement>());
        }
    }

    public override void ExitProcDecl(DelphiParser.ProcDeclContext context)
    {
        if (_currentProcedure == null) return;

        // Pop the procedure body statements from stack if available
        if (!_currentProcedure.IsForwardDeclaration && _statementStack.Count > 0)
        {
            var body = _statementStack.Pop();
            _currentProcedure.Body = body;
        }

        if (_inInterface)
        {
            Unit.InterfaceSection.Procedures.Add(_currentProcedure);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Procedures.Add(_currentProcedure);
        }

        _currentProcedure = null;
        _statementStack.Clear();
    }

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
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitMethodDecl(DelphiParser.MethodDeclContext context)
    {
        if (_currentProcedure == null) return;
        
        // Pop the method body statements from stack
        if (_statementStack.Count > 0)
        {
            var body = _statementStack.Pop();
            _currentProcedure.Body = body;
        }
        
        if (_inImplementation)
        {
            Unit.ImplementationSection.Procedures.Add(_currentProcedure);
        }

        _currentProcedure = null;
        _statementStack.Clear();
    }

    public override void EnterCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var statements = _statementStack.Pop();
        
        // If we're at the method body level (stack count == 1), add statements directly without wrapping
        if (_statementStack.Count == 1)
        {
            foreach (var stmt in statements)
            {
                _statementStack.Peek().Add(stmt);
            }
        }
        else if (_statementStack.Count > 0)
        {
            var compound = new AstCompoundStatement { Statements = statements, Span = GetSpan(context) };
            _statementStack.Peek().Add(compound);
        }
    }

    public override void ExitSimpleStatement(DelphiParser.SimpleStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var text = context.GetText();
        
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

    public override void ExitIfStatement(DelphiParser.IfStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var ifStmt = new AstIfStatement
        {
            Condition = context.expression()?.GetText() ?? string.Empty,
            ThenBranch = new List<AstStatement>(),
            ElseBranch = new List<AstStatement>(),
            Span = GetSpan(context)
        };

        var statements = context.statement();
        if (statements.Length > 0)
        {
            ifStmt.ThenBranch = ExtractStatementsFromContext(statements[0]);
        }
        if (statements.Length > 1)
        {
            ifStmt.ElseBranch = ExtractStatementsFromContext(statements[1]);
        }

        _statementStack.Peek().Add(ifStmt);
    }

    public override void ExitWhileStatement(DelphiParser.WhileStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var whileStmt = new AstWhileStatement
        {
            Condition = context.expression()?.GetText() ?? string.Empty,
            Body = ExtractStatementsFromContext(context.statement()),
            Span = GetSpan(context)
        };

        _statementStack.Peek().Add(whileStmt);
    }

    public override void ExitForStatement(DelphiParser.ForStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var forStmt = new AstForStatement
        {
            LoopVariable = context.designator()?.GetText() ?? string.Empty,
            Span = GetSpan(context)
        };

        var expressions = context.expression();
        if (context.GetText().Contains(" in "))
        {
            forStmt.IsForIn = true;
            forStmt.Collection = expressions.Length > 0 ? expressions[0].GetText() : string.Empty;
        }
        else
        {
            forStmt.StartValue = expressions.Length > 0 ? expressions[0].GetText() : string.Empty;
            forStmt.EndValue = expressions.Length > 1 ? expressions[1].GetText() : string.Empty;
            forStmt.Direction = context.GetText().Contains(" to ") ? "to" : "downto";
        }

        forStmt.Body = ExtractStatementsFromContext(context.statement());
        _statementStack.Peek().Add(forStmt);
    }

    public override void ExitWithStatement(DelphiParser.WithStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var withStmt = new AstWithStatement
        {
            WithItems = new List<string>(),
            Body = ExtractStatementsFromContext(context.statement()),
            Span = GetSpan(context)
        };

        var withItem = context.withItem();
        if (withItem != null)
        {
            foreach (var designator in withItem.designator())
            {
                withStmt.WithItems.Add(designator.GetText());
            }
        }

        _statementStack.Peek().Add(withStmt);
    }

    public override void ExitTryStatement(DelphiParser.TryStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var tryStmt = new AstTryStatement
        {
            TryBlock = new List<AstStatement>(),
            FinallyBlock = new List<AstStatement>(),
            ExceptHandlers = new List<AstExceptionHandler>(),
            Span = GetSpan(context)
        };

        var statementLists = context.statementList();
        if (statementLists.Length > 0)
        {
            tryStmt.TryBlock = ExtractStatementsFromStatementList(statementLists[0]);
        }

        var contextText = context.GetText();
        if (contextText.Contains("finally"))
        {
            tryStmt.IsTryFinally = true;
            if (statementLists.Length > 1)
            {
                tryStmt.FinallyBlock = ExtractStatementsFromStatementList(statementLists[1]);
            }
        }
        else if (contextText.Contains("except"))
        {
            tryStmt.IsTryFinally = false;
            var handlerList = context.handlerList();
            if (handlerList != null)
            {
                foreach (var handler in handlerList.handler())
                {
                    var exHandler = new AstExceptionHandler
                    {
                        VariableName = handler.handlerIdent()?.ident()?.GetText() ?? string.Empty,
                        ExceptionType = handler.typeId()?.GetText() ?? string.Empty,
                        Body = handler.handlerStatement()?.statement() != null 
                            ? ExtractStatementsFromContext(handler.handlerStatement()!.statement()!)
                            : new List<AstStatement>()
                    };
                    tryStmt.ExceptHandlers.Add(exHandler);
                }
                
                if (statementLists.Length > 1)
                {
                    var elseHandler = new AstExceptionHandler
                    {
                        VariableName = string.Empty,
                        ExceptionType = "else",
                        Body = ExtractStatementsFromStatementList(statementLists[1])
                    };
                    tryStmt.ExceptHandlers.Add(elseHandler);
                }
            }
        }

        _statementStack.Peek().Add(tryStmt);
    }

    private List<AstParameter> ParseParameters(DelphiParser.FormalParameterSectionContext? formalParams)
    {
        var parameters = new List<AstParameter>();
        if (formalParams == null) return parameters;

        var paramList = formalParams.formalParameterList();
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

    private List<AstStatement> ExtractStatementsFromContext(DelphiParser.StatementContext stmt)
    {
        if (stmt == null) return new List<AstStatement>();
        
        var result = new List<AstStatement>();
        var text = stmt.GetText();
        
        if (text.Contains(":="))
        {
            var parts = text.Split(new[] { ":=" }, 2, System.StringSplitOptions.None);
            if (parts.Length == 2)
            {
                result.Add(new AstAssignment 
                { 
                    Target = parts[0], 
                    Value = parts[1],
                    Span = GetSpan(stmt)
                });
            }
        }
        else
        {
            result.Add(new AstProcedureCall 
            { 
                ProcedureName = text.Split('(')[0],
                Arguments = new List<string>(),
                Span = GetSpan(stmt)
            });
        }
        
        return result;
    }

    private List<AstStatement> ExtractStatementsFromStatementList(DelphiParser.StatementListContext stmtList)
    {
        if (stmtList == null) return new List<AstStatement>();
        
        var result = new List<AstStatement>();
        foreach (var stmt in stmtList.statement())
        {
            if (stmt != null)
            {
                result.AddRange(ExtractStatementsFromContext(stmt));
            }
        }
        
        return result;
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
