using Transpiler.AST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Linq;
using System.Collections.Generic;
using DelphiGrammar;

// Enhanced listener that builds a structured AstUnit for CalendarController
public class CalendarControllerAstListener : DelphiBaseListener
{
    public AstUnit Unit { get; } = new AstUnit { Name = "CalendarController" };
    
    private bool _inInterface = false;
    private bool _inImplementation = false;
    private AstProcedure? _currentProcedure = null;
    private Stack<List<AstStatement>> _statementStack = new Stack<List<AstStatement>>();

    // Track which section we're in
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

    // Parse uses clause
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

    // Parse constant declarations
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

    // Parse variable declarations
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

    // Parse type declarations
    public override void ExitTypeDeclaration(DelphiParser.TypeDeclarationContext context)
    {
        var typeIdent = context.genericTypeIdent();
        if (typeIdent == null) return;

        var typeDecl = context.typeDecl();
        if (typeDecl == null) return;

        var astTypeDecl = new AstTypeDeclaration
        {
            Name = typeIdent.GetText(),
            TypeKind = DetermineTypeKind(typeDecl),
            BaseType = typeDecl.GetText(),
            Span = GetSpan(context)
        };

        // Check if it's a class/record type
        var strucType = typeDecl.strucType();
        if (strucType != null)
        {
            var classDecl = strucType.strucTypePart()?.classDecl();
            if (classDecl != null)
            {
                ParseClassDetails(classDecl, astTypeDecl);
            }
        }

        if (_currentProcedure != null)
        {
            _currentProcedure.LocalTypes.Add(astTypeDecl);
        }
        else if (_inInterface)
        {
            Unit.InterfaceSection.Types.Add(astTypeDecl);
        }
        else if (_inImplementation)
        {
            Unit.ImplementationSection.Types.Add(astTypeDecl);
        }
    }

    // Parse procedure/function declarations
    public override void EnterProcDecl(DelphiParser.ProcDeclContext context)
    {
        var heading = context.procDeclHeading();
        if (heading == null) return;

        var ident = heading.ident();
        if (ident == null) return;

        // Determine if it's a function or procedure by checking the heading text
        var headingText = heading.GetText();
        var isFunction = headingText.StartsWith("function", System.StringComparison.OrdinalIgnoreCase);

        _currentProcedure = new AstProcedure
        {
            Name = ident.GetText(),
            ProcedureType = isFunction ? "function" : "procedure",
            ReturnType = heading.typeDecl()?.GetText() ?? string.Empty,
            Parameters = ParseParameters(heading.formalParameterSection()),
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
        
        // Initialize statement stack for capturing procedure body
        _statementStack.Clear();
        _statementStack.Push(new List<AstStatement>());
        System.Console.WriteLine($"[DEBUG CalendarController] EnterProcDecl: {ident.GetText()}, pre-pushed stack level");
    }

    public override void ExitProcDecl(DelphiParser.ProcDeclContext context)
    {
        if (_currentProcedure == null) return;

        // Pop and assign body statements
        if (_statementStack.Count > 0)
        {
            var body = _statementStack.Pop();
            System.Console.WriteLine($"[DEBUG CalendarController] ExitProcDecl: {_currentProcedure.Name}, popped {body.Count} statements");
            _currentProcedure.Body = body;
        }

        // Add to appropriate section
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

    // Parse block (contains local declarations and statements)
    public override void EnterBlock(DelphiParser.BlockContext context)
    {
        // Process local declarations in procedure
        if (_currentProcedure != null)
        {
            foreach (var declSection in context.declSection())
            {
                // Constants, types, and variables are already handled by their exit methods
            }
        }
    }

    // Parse statements in procedure body
    public override void EnterCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        // Start a new statement list for this compound block
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitCompoundStatement(DelphiParser.CompoundStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var statements = _statementStack.Pop();
        System.Console.WriteLine($"[DEBUG CalendarController] ExitCompoundStatement: Popped {statements.Count} statements, stack now has {_statementStack.Count} levels");
        
        // If we're at the method body level (stack count == 1), add statements directly without wrapping
        if (_statementStack.Count == 1)
        {
            System.Console.WriteLine($"[DEBUG CalendarController] Adding {statements.Count} statements directly to method body (no wrapping)");
            foreach (var stmt in statements)
            {
                _statementStack.Peek().Add(stmt);
            }
        }
        else if (_statementStack.Count > 0)
        {
            // Nested compound statement - wrap it
            var compound = new AstCompoundStatement { Statements = statements, Span = GetSpan(context) };
            System.Console.WriteLine($"[DEBUG CalendarController] Wrapping {statements.Count} statements in compound");
            _statementStack.Peek().Add(compound);
        }
    }

    // Parse assignment statements and procedure calls
    public override void ExitSimpleStatement(DelphiParser.SimpleStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var text = context.GetText();
        
        // Check if it's an assignment
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

        // It's a procedure call or other simple statement
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

    // Parse if statements  
    public override void EnterIfStatement(DelphiParser.IfStatementContext context)
    {
        // Push stack for then/else branches
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitIfStatement(DelphiParser.IfStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        // Pop the collected statements (may include then + else)
        var branchStatements = _statementStack.Pop();

        var ifStmt = new AstIfStatement
        {
            Condition = context.expression()?.GetText() ?? string.Empty,
            ThenBranch = new List<AstStatement>(),
            ElseBranch = new List<AstStatement>(),
            Span = GetSpan(context)
        };

        // For now, assign all collected statements to then branch
        // Properly splitting then/else requires more sophisticated tracking
        ifStmt.ThenBranch = branchStatements;

        _statementStack.Peek().Add(ifStmt);
    }

    // Parse while statements
    public override void EnterWhileStatement(DelphiParser.WhileStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitWhileStatement(DelphiParser.WhileStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        // The body statements have been pushed to stack by EnterCompoundStatement or similar
        var body = _statementStack.Pop();

        var whileStmt = new AstWhileStatement
        {
            Condition = context.expression()?.GetText() ?? string.Empty,
            Body = body,
            Span = GetSpan(context)
        };

        _statementStack.Peek().Add(whileStmt);
    }

    // Parse for statements
    public override void EnterForStatement(DelphiParser.ForStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitForStatement(DelphiParser.ForStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        var body = _statementStack.Pop();

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

        // Body will be populated by ExitCompoundStatement if the statement is a begin-end block
        // Otherwise we leave it empty as we can't properly extract it from raw text
        _statementStack.Peek().Add(forStmt);
    }

    // Parse with statements
    public override void EnterWithStatement(DelphiParser.WithStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitWithStatement(DelphiParser.WithStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        // The body statements have been pushed to stack by EnterCompoundStatement
        // Pop the body (created by the compound statement inside the with block)
        var body = _statementStack.Pop();

        var withStmt = new AstWithStatement
        {
            WithItems = new List<string>(),
            Body = body,
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

    // Parse try statements
    public override void EnterTryStatement(DelphiParser.TryStatementContext context)
    {
        // Push stack for try block
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitTryStatement(DelphiParser.TryStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        // Pop the try block statements
        var tryBlockStatements = _statementStack.Pop();

        var tryStmt = new AstTryStatement
        {
            TryBlock = tryBlockStatements,
            FinallyBlock = new List<AstStatement>(),
            ExceptHandlers = new List<AstExceptionHandler>(),
            Span = GetSpan(context)
        };

        // Check if it's try-finally or try-except by looking at the text
        var contextText = context.GetText();
        if (contextText.Contains("finally"))
        {
            tryStmt.IsTryFinally = true;
        }
        else if (contextText.Contains("except"))
        {
            tryStmt.IsTryFinally = false;
        }

        _statementStack.Peek().Add(tryStmt);
    }

    // Parse case statements
    public override void EnterCaseStatement(DelphiParser.CaseStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitCaseStatement(DelphiParser.CaseStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        var caseBodyStatements = _statementStack.Pop();

        var caseStmt = new AstCaseStatement
        {
            Expression = context.expression()?.GetText() ?? string.Empty,
            Cases = new List<AstCaseItem>(),
            ElseBranch = new List<AstStatement>(),
            Span = GetSpan(context)
        };

        foreach (var caseItem in context.caseItem())
        {
            var astCase = new AstCaseItem
            {
                Labels = caseItem.caseLabel().Select(l => l.GetText()).ToList(),
                Body = new List<AstStatement>()
            };
            caseStmt.Cases.Add(astCase);
        }

        // Assign collected statements to case statement
        // Properly splitting case branches requires more tracking
        caseStmt.ElseBranch = caseBodyStatements;

        _statementStack.Peek().Add(caseStmt);
    }

    // Parse repeat statements
    public override void EnterRepeatStatement(DelphiParser.RepeatStatementContext context)
    {
        _statementStack.Push(new List<AstStatement>());
    }

    public override void ExitRepeatStatement(DelphiParser.RepeatStatementContext context)
    {
        if (_statementStack.Count < 2) return;

        var body = _statementStack.Pop();

        var repeatStmt = new AstRepeatStatement
        {
            Condition = context.expression()?.GetText() ?? string.Empty,
            Body = body,
            Span = GetSpan(context)
        };

        _statementStack.Peek().Add(repeatStmt);
    }

    // Parse raise statements
    public override void ExitRaiseStatement(DelphiParser.RaiseStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var raiseStmt = new AstRaiseStatement
        {
            Exception = string.Empty,
            AtAddress = string.Empty,
            Span = GetSpan(context)
        };

        var designators = context.designator();
        if (designators.Length > 0)
        {
            raiseStmt.Exception = designators[0].GetText();
        }
        if (designators.Length > 1)
        {
            raiseStmt.AtAddress = designators[1].GetText();
        }

        _statementStack.Peek().Add(raiseStmt);
    }

    // Parse goto/exit/break/continue
    public override void ExitGotoStatement(DelphiParser.GotoStatementContext context)
    {
        if (_statementStack.Count == 0) return;

        var controlStmt = new AstControlFlowStatement
        {
            ControlType = string.Empty,
            Target = string.Empty,
            Span = GetSpan(context)
        };

        var text = context.GetText().ToLower();
        if (text.StartsWith("goto"))
        {
            controlStmt.ControlType = "goto";
            controlStmt.Target = context.label()?.GetText() ?? string.Empty;
        }
        else if (text.StartsWith("exit"))
        {
            controlStmt.ControlType = "exit";
            controlStmt.Target = context.expression()?.GetText() ?? string.Empty;
        }
        else if (text.StartsWith("break"))
        {
            controlStmt.ControlType = "break";
        }
        else if (text.StartsWith("continue"))
        {
            controlStmt.ControlType = "continue";
        }

        _statementStack.Peek().Add(controlStmt);
    }

    // Helper methods
    private List<AstParameter> ParseParameters(DelphiParser.FormalParameterSectionContext context)
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

    private string DetermineTypeKind(DelphiParser.TypeDeclContext context)
    {
        if (context.strucType() != null)
        {
            var strucPart = context.strucType().strucTypePart();
            if (strucPart != null)
            {
                if (strucPart.arrayType() != null) return "array";
                if (strucPart.setType() != null) return "set";
                if (strucPart.fileType() != null) return "file";
                if (strucPart.classDecl() != null)
                {
                    var classDecl = strucPart.classDecl();
                    if (classDecl.classTypeDecl() != null) return "class";
                    if (classDecl.interfaceTypeDecl() != null) return "interface";
                    if (classDecl.recordDecl() != null) return "record";
                    if (classDecl.objectDecl() != null) return "object";
                }
            }
        }
        if (context.pointerType() != null) return "pointer";
        if (context.stringType() != null) return "string";
        if (context.procedureType() != null) return "procedureType";
        if (context.simpleType() != null) return "alias";
        
        return "alias";
    }

    private void ParseClassDetails(DelphiParser.ClassDeclContext classDecl, AstTypeDeclaration astType)
    {
        // This could be expanded to parse class members if needed
        // For now, we just mark it as processed
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
        return designator.GetText().Split('(')[0]; // Get name before any parentheses
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
