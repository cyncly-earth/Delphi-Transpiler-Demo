using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Transpiler.AST
{
    // Root unit node representing a Delphi unit
    public class AstUnit
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        // Interface section
        [JsonPropertyName("interfaceSection")]
        public AstSection InterfaceSection { get; set; } = new();
        
        // Implementation section
        [JsonPropertyName("implementationSection")]
        public AstSection ImplementationSection { get; set; } = new();
        
        // Initialization/finalization blocks (optional)
        [JsonPropertyName("initializationStatements")]
        public List<AstStatement> InitializationStatements { get; set; } = new();
        
        [JsonPropertyName("finalizationStatements")]
        public List<AstStatement> FinalizationStatements { get; set; } = new();
    }

    // Represents interface or implementation section
    public class AstSection
    {
        [JsonPropertyName("uses")]
        public List<string> Uses { get; set; } = new(); // imported units
        
        [JsonPropertyName("constants")]
        public List<AstConstDeclaration> Constants { get; set; } = new();
        
        [JsonPropertyName("types")]
        public List<AstTypeDeclaration> Types { get; set; } = new();
        
        [JsonPropertyName("variables")]
        public List<AstVarDeclaration> Variables { get; set; } = new();
        
        [JsonPropertyName("classes")]
        public List<AstClass> Classes { get; set; } = new();
        
        [JsonPropertyName("procedures")]
        public List<AstProcedure> Procedures { get; set; } = new();
    }

    // Constant declaration
    public class AstConstDeclaration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty; // optional explicit type
        
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty; // constant expression
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Type declaration
    public class AstTypeDeclaration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("typeKind")]
        public string TypeKind { get; set; } = string.Empty; // "class", "record", "array", "pointer", etc.
        
        [JsonPropertyName("baseType")]
        public string BaseType { get; set; } = string.Empty; // for aliases or pointers
        
        [JsonPropertyName("parentTypes")]
        public List<string> ParentTypes { get; set; } = new(); // for classes/interfaces
        
        [JsonPropertyName("fields")]
        public List<AstField> Fields { get; set; } = new();
        
        [JsonPropertyName("methods")]
        public List<AstProcedure> Methods { get; set; } = new();
        
        [JsonPropertyName("properties")]
        public List<AstProperty> Properties { get; set; } = new();
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Variable declaration
    public class AstVarDeclaration
    {
        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new(); // can declare multiple vars at once
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("initialValue")]
        public string InitialValue { get; set; } = string.Empty; // optional
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Field (class/record field)
    public class AstField
    {
        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new();
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("visibility")]
        public string Visibility { get; set; } = string.Empty; // "private", "public", etc.
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Property (class property)
    public class AstProperty
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("readSpecifier")]
        public string ReadSpecifier { get; set; } = string.Empty;
        
        [JsonPropertyName("writeSpecifier")]
        public string WriteSpecifier { get; set; } = string.Empty;
        
        [JsonPropertyName("visibility")]
        public string Visibility { get; set; } = string.Empty;
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Class declaration
    public class AstClass
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("classType")]
        public string ClassType { get; set; } = string.Empty; // "class", "interface", "record"
        
        [JsonPropertyName("parentTypes")]
        public List<string> ParentTypes { get; set; } = new();
        
        [JsonPropertyName("fields")]
        public List<AstField> Fields { get; set; } = new();
        
        [JsonPropertyName("methods")]
        public List<AstProcedure> Methods { get; set; } = new();
        
        [JsonPropertyName("properties")]
        public List<AstProperty> Properties { get; set; } = new();
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Procedure/function/method declaration
    public class AstProcedure
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("procedureType")]
        public string ProcedureType { get; set; } = string.Empty; // "procedure", "function", "constructor", "destructor"
        
        [JsonPropertyName("parameters")]
        public List<AstParameter> Parameters { get; set; } = new();
        
        [JsonPropertyName("returnType")]
        public string ReturnType { get; set; } = string.Empty;
        
        [JsonPropertyName("isForwardDeclaration")]
        public bool IsForwardDeclaration { get; set; } = false;
        
        [JsonPropertyName("visibility")]
        public string Visibility { get; set; } = string.Empty; // for methods
        
        [JsonPropertyName("directives")]
        public List<string> Directives { get; set; } = new(); // "virtual", "override", "inline", etc.
        
        // Local declarations within the procedure
        [JsonPropertyName("localConstants")]
        public List<AstConstDeclaration> LocalConstants { get; set; } = new();
        
        [JsonPropertyName("localVariables")]
        public List<AstVarDeclaration> LocalVariables { get; set; } = new();
        
        [JsonPropertyName("localTypes")]
        public List<AstTypeDeclaration> LocalTypes { get; set; } = new();
        
        // Procedure body (structured statements)
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
        
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Parameter
    public class AstParameter
    {
        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new();
        
        [JsonPropertyName("parameterType")]
        public string ParameterType { get; set; } = string.Empty; // "var", "const", "out", or empty
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("defaultValue")]
        public string DefaultValue { get; set; } = string.Empty;
    }

    // Base statement class
    [JsonDerivedType(typeof(AstAssignment), typeDiscriminator: "assignment")]
    [JsonDerivedType(typeof(AstProcedureCall), typeDiscriminator: "procedureCall")]
    [JsonDerivedType(typeof(AstIfStatement), typeDiscriminator: "ifStatement")]
    [JsonDerivedType(typeof(AstWhileStatement), typeDiscriminator: "whileStatement")]
    [JsonDerivedType(typeof(AstForStatement), typeDiscriminator: "forStatement")]
    [JsonDerivedType(typeof(AstWithStatement), typeDiscriminator: "withStatement")]
    [JsonDerivedType(typeof(AstTryStatement), typeDiscriminator: "tryStatement")]
    [JsonDerivedType(typeof(AstCaseStatement), typeDiscriminator: "caseStatement")]
    [JsonDerivedType(typeof(AstRepeatStatement), typeDiscriminator: "repeatStatement")]
    [JsonDerivedType(typeof(AstCompoundStatement), typeDiscriminator: "compoundStatement")]
    [JsonDerivedType(typeof(AstRaiseStatement), typeDiscriminator: "raiseStatement")]
    [JsonDerivedType(typeof(AstControlFlowStatement), typeDiscriminator: "controlFlow")]
    public abstract class AstStatement
    {
        [JsonPropertyName("span")]
        public SourceSpan Span { get; set; } = new();
    }

    // Assignment statement
    public class AstAssignment : AstStatement
    {
        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty; // LHS (designator)
        
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty; // RHS (expression)
    }

    // Procedure call statement
    public class AstProcedureCall : AstStatement
    {
        [JsonPropertyName("procedureName")]
        public string ProcedureName { get; set; } = string.Empty;
        
        [JsonPropertyName("arguments")]
        public List<string> Arguments { get; set; } = new();
    }

    // If statement
    public class AstIfStatement : AstStatement
    {
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;
        
        [JsonPropertyName("thenBranch")]
        public List<AstStatement> ThenBranch { get; set; } = new();
        
        [JsonPropertyName("elseBranch")]
        public List<AstStatement> ElseBranch { get; set; } = new();
    }

    // While statement
    public class AstWhileStatement : AstStatement
    {
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // For statement
    public class AstForStatement : AstStatement
    {
        [JsonPropertyName("loopVariable")]
        public string LoopVariable { get; set; } = string.Empty;
        
        [JsonPropertyName("startValue")]
        public string StartValue { get; set; } = string.Empty;
        
        [JsonPropertyName("endValue")]
        public string EndValue { get; set; } = string.Empty;
        
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty; // "to" or "downto"
        
        [JsonPropertyName("isForIn")]
        public bool IsForIn { get; set; } = false; // for-in loop
        
        [JsonPropertyName("collection")]
        public string Collection { get; set; } = string.Empty; // for for-in loops
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // With statement
    public class AstWithStatement : AstStatement
    {
        [JsonPropertyName("withItems")]
        public List<string> WithItems { get; set; } = new();
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // Try-except/try-finally statement
    public class AstTryStatement : AstStatement
    {
        [JsonPropertyName("tryBlock")]
        public List<AstStatement> TryBlock { get; set; } = new();
        
        [JsonPropertyName("exceptHandlers")]
        public List<AstExceptionHandler> ExceptHandlers { get; set; } = new();
        
        [JsonPropertyName("finallyBlock")]
        public List<AstStatement> FinallyBlock { get; set; } = new();
        
        [JsonPropertyName("isTryFinally")]
        public bool IsTryFinally { get; set; } = false;
    }

    // Exception handler
    public class AstExceptionHandler
    {
        [JsonPropertyName("variableName")]
        public string VariableName { get; set; } = string.Empty;
        
        [JsonPropertyName("exceptionType")]
        public string ExceptionType { get; set; } = string.Empty;
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // Case statement
    public class AstCaseStatement : AstStatement
    {
        [JsonPropertyName("expression")]
        public string Expression { get; set; } = string.Empty;
        
        [JsonPropertyName("cases")]
        public List<AstCaseItem> Cases { get; set; } = new();
        
        [JsonPropertyName("elseBranch")]
        public List<AstStatement> ElseBranch { get; set; } = new();
    }

    // Case item
    public class AstCaseItem
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new();
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // Repeat-until statement
    public class AstRepeatStatement : AstStatement
    {
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;
        
        [JsonPropertyName("body")]
        public List<AstStatement> Body { get; set; } = new();
    }

    // Compound statement (begin-end block)
    public class AstCompoundStatement : AstStatement
    {
        [JsonPropertyName("statements")]
        public List<AstStatement> Statements { get; set; } = new();
    }

    // Raise statement
    public class AstRaiseStatement : AstStatement
    {
        [JsonPropertyName("exception")]
        public string Exception { get; set; } = string.Empty;
        
        [JsonPropertyName("atAddress")]
        public string AtAddress { get; set; } = string.Empty;
    }

    // Exit/break/continue/goto
    public class AstControlFlowStatement : AstStatement
    {
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty; // "exit", "break", "continue", "goto"
        
        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty; // for goto labels or exit expressions
    }

    // Source location span
    public class SourceSpan
    {
        [JsonPropertyName("startLine")]
        public int StartLine { get; set; }
        
        [JsonPropertyName("startColumn")]
        public int StartColumn { get; set; }
        
        [JsonPropertyName("endLine")]
        public int EndLine { get; set; }
        
        [JsonPropertyName("endColumn")]
        public int EndColumn { get; set; }
    }
}
