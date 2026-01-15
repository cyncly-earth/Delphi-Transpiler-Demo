using System.Collections.Generic;

namespace Transpiler.AST
{
    // Root unit node representing a Delphi unit
    public class AstUnit
    {
        public string Name { get; set; } = string.Empty;
        
        // Interface section
        public AstSection InterfaceSection { get; set; } = new();
        
        // Implementation section
        public AstSection ImplementationSection { get; set; } = new();
        
        // Initialization/finalization blocks (optional)
        public List<AstStatement> InitializationStatements { get; set; } = new();
        public List<AstStatement> FinalizationStatements { get; set; } = new();
    }

    // Represents interface or implementation section
    public class AstSection
    {
        public List<string> Uses { get; set; } = new(); // imported units
        public List<AstConstDeclaration> Constants { get; set; } = new();
        public List<AstTypeDeclaration> Types { get; set; } = new();
        public List<AstVarDeclaration> Variables { get; set; } = new();
        public List<AstClass> Classes { get; set; } = new();
        public List<AstProcedure> Procedures { get; set; } = new();
    }

    // Constant declaration
    public class AstConstDeclaration
    {
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty; // optional explicit type
        public string Value { get; set; } = string.Empty; // constant expression
        public SourceSpan Span { get; set; } = new();
    }

    // Type declaration
    public class AstTypeDeclaration
    {
        public string Name { get; set; } = string.Empty;
        public string TypeKind { get; set; } = string.Empty; // "class", "record", "array", "pointer", etc.
        public string BaseType { get; set; } = string.Empty; // for aliases or pointers
        public List<string> ParentTypes { get; set; } = new(); // for classes/interfaces
        public List<AstField> Fields { get; set; } = new();
        public List<AstProcedure> Methods { get; set; } = new();
        public List<AstProperty> Properties { get; set; } = new();
        public SourceSpan Span { get; set; } = new();
    }

    // Variable declaration
    public class AstVarDeclaration
    {
        public List<string> Names { get; set; } = new(); // can declare multiple vars at once
        public string TypeName { get; set; } = string.Empty;
        public string InitialValue { get; set; } = string.Empty; // optional
        public SourceSpan Span { get; set; } = new();
    }

    // Field (class/record field)
    public class AstField
    {
        public List<string> Names { get; set; } = new();
        public string TypeName { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty; // "private", "public", etc.
        public SourceSpan Span { get; set; } = new();
    }

    // Property (class property)
    public class AstProperty
    {
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string ReadSpecifier { get; set; } = string.Empty;
        public string WriteSpecifier { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public SourceSpan Span { get; set; } = new();
    }

    // Class declaration
    public class AstClass
    {
        public string Name { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty; // "class", "interface", "record"
        public List<string> ParentTypes { get; set; } = new();
        public List<AstField> Fields { get; set; } = new();
        public List<AstProcedure> Methods { get; set; } = new();
        public List<AstProperty> Properties { get; set; } = new();
        public SourceSpan Span { get; set; } = new();
    }

    // Procedure/function/method declaration
    public class AstProcedure
    {
        public string Name { get; set; } = string.Empty;
        public string ProcedureType { get; set; } = string.Empty; // "procedure", "function", "constructor", "destructor"
        public List<AstParameter> Parameters { get; set; } = new();
        public string ReturnType { get; set; } = string.Empty;
        public bool IsForwardDeclaration { get; set; } = false;
        public string Visibility { get; set; } = string.Empty; // for methods
        public List<string> Directives { get; set; } = new(); // "virtual", "override", "inline", etc.
        
        // Local declarations within the procedure
        public List<AstConstDeclaration> LocalConstants { get; set; } = new();
        public List<AstVarDeclaration> LocalVariables { get; set; } = new();
        public List<AstTypeDeclaration> LocalTypes { get; set; } = new();
        
        // Procedure body (structured statements)
        public List<AstStatement> Body { get; set; } = new();
        
        public SourceSpan Span { get; set; } = new();
    }

    // Parameter
    public class AstParameter
    {
        public List<string> Names { get; set; } = new();
        public string ParameterType { get; set; } = string.Empty; // "var", "const", "out", or empty
        public string TypeName { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
    }

    // Base statement class
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstAssignment), typeDiscriminator: "assignment")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstProcedureCall), typeDiscriminator: "procedureCall")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstIfStatement), typeDiscriminator: "ifStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstWhileStatement), typeDiscriminator: "whileStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstForStatement), typeDiscriminator: "forStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstWithStatement), typeDiscriminator: "withStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstTryStatement), typeDiscriminator: "tryStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstCaseStatement), typeDiscriminator: "caseStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstRepeatStatement), typeDiscriminator: "repeatStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstCompoundStatement), typeDiscriminator: "compoundStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstRaiseStatement), typeDiscriminator: "raiseStatement")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(AstControlFlowStatement), typeDiscriminator: "controlFlow")]
    public abstract class AstStatement
    {
        public SourceSpan Span { get; set; } = new();
    }

    // Assignment statement
    public class AstAssignment : AstStatement
    {
        public string Target { get; set; } = string.Empty; // LHS (designator)
        public string Value { get; set; } = string.Empty; // RHS (expression)
    }

    // Procedure call statement
    public class AstProcedureCall : AstStatement
    {
        public string ProcedureName { get; set; } = string.Empty;
        public List<string> Arguments { get; set; } = new();
    }

    // If statement
    public class AstIfStatement : AstStatement
    {
        public string Condition { get; set; } = string.Empty;
        public List<AstStatement> ThenBranch { get; set; } = new();
        public List<AstStatement> ElseBranch { get; set; } = new();
    }

    // While statement
    public class AstWhileStatement : AstStatement
    {
        public string Condition { get; set; } = string.Empty;
        public List<AstStatement> Body { get; set; } = new();
    }

    // For statement
    public class AstForStatement : AstStatement
    {
        public string LoopVariable { get; set; } = string.Empty;
        public string StartValue { get; set; } = string.Empty;
        public string EndValue { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty; // "to" or "downto"
        public bool IsForIn { get; set; } = false; // for-in loop
        public string Collection { get; set; } = string.Empty; // for for-in loops
        public List<AstStatement> Body { get; set; } = new();
    }

    // With statement
    public class AstWithStatement : AstStatement
    {
        public List<string> WithItems { get; set; } = new();
        public List<AstStatement> Body { get; set; } = new();
    }

    // Try-except/try-finally statement
    public class AstTryStatement : AstStatement
    {
        public List<AstStatement> TryBlock { get; set; } = new();
        public List<AstExceptionHandler> ExceptHandlers { get; set; } = new();
        public List<AstStatement> FinallyBlock { get; set; } = new();
        public bool IsTryFinally { get; set; } = false;
    }

    // Exception handler
    public class AstExceptionHandler
    {
        public string VariableName { get; set; } = string.Empty;
        public string ExceptionType { get; set; } = string.Empty;
        public List<AstStatement> Body { get; set; } = new();
    }

    // Case statement
    public class AstCaseStatement : AstStatement
    {
        public string Expression { get; set; } = string.Empty;
        public List<AstCaseItem> Cases { get; set; } = new();
        public List<AstStatement> ElseBranch { get; set; } = new();
    }

    // Case item
    public class AstCaseItem
    {
        public List<string> Labels { get; set; } = new();
        public List<AstStatement> Body { get; set; } = new();
    }

    // Repeat-until statement
    public class AstRepeatStatement : AstStatement
    {
        public string Condition { get; set; } = string.Empty;
        public List<AstStatement> Body { get; set; } = new();
    }

    // Compound statement (begin-end block)
    public class AstCompoundStatement : AstStatement
    {
        public List<AstStatement> Statements { get; set; } = new();
    }

    // Raise statement
    public class AstRaiseStatement : AstStatement
    {
        public string Exception { get; set; } = string.Empty;
        public string AtAddress { get; set; } = string.Empty;
    }

    // Exit/break/continue/goto
    public class AstControlFlowStatement : AstStatement
    {
        public string ControlType { get; set; } = string.Empty; // "exit", "break", "continue", "goto"
        public string Target { get; set; } = string.Empty; // for goto labels or exit expressions
    }

    // Source location span
    public class SourceSpan
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
