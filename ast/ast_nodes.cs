// /workspaces/Delphi-Transpiler-Demo/ast/ast_nodes.cs

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DelphiTranspiler.AST
{
    // Base class for all AST nodes
    public abstract class AstNode
    {
        [JsonPropertyName("nodeType")]
        public string NodeType => GetType().Name;
        
        [JsonPropertyName("location")]
        public SourceLocation? Location { get; set; }
    }

    public class SourceLocation
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string? File { get; set; }
    }

    // ========== COMPILATION UNIT ==========
    public class CompilationUnitNode : AstNode
    {
        public string Name { get; set; } = "";
        public string UnitType { get; set; } = "unit";
        public List<UsesClauseNode> InterfaceUses { get; set; } = new();
        public List<UsesClauseNode> ImplementationUses { get; set; } = new();
        public List<TypeDeclarationNode> TypeDeclarations { get; set; } = new();
        public List<VariableDeclarationNode> GlobalVariables { get; set; } = new();
        public List<ProcedureNode> Procedures { get; set; } = new();
        public List<FunctionNode> Functions { get; set; } = new();
        public List<ConstantDeclarationNode> Constants { get; set; } = new();
    }

    // ========== DECLARATIONS ==========
    public class UsesClauseNode : AstNode
    {
        public List<string> Units { get; set; } = new();
    }

    public class TypeDeclarationNode : AstNode
    {
        public string Name { get; set; } = "";
        public TypeNode? Type { get; set; }
    }

    public class ClassDeclarationNode : AstNode
    {
        public string Name { get; set; } = "";
        public List<string> Ancestors { get; set; } = new();
        public Visibility DefaultVisibility { get; set; } = Visibility.Public;
        public List<FieldDeclarationNode> Fields { get; set; } = new();
        public List<PropertyDeclarationNode> Properties { get; set; } = new();
        public List<MethodDeclarationNode> Methods { get; set; } = new();
    }

    public enum Visibility
    {
        Private,
        Protected,
        Public,
        Published
    }

    // ========== MEMBERS ==========
    public class FieldDeclarationNode : AstNode
    {
        public List<string> Names { get; set; } = new();
        public TypeNode? Type { get; set; }
        public Visibility Visibility { get; set; }
    }

    public class PropertyDeclarationNode : AstNode
    {
        public string Name { get; set; } = "";
        public TypeNode? Type { get; set; }
        public PropertyAccessor? Getter { get; set; }
        public PropertyAccessor? Setter { get; set; }
    }

    public class PropertyAccessor
    {
        public string Name { get; set; } = "";
        public bool IsDirectField { get; set; } = true;
    }

    public class MethodDeclarationNode : AstNode
    {
        public string Name { get; set; } = "";
        public MethodKind Kind { get; set; }
        public TypeNode? ReturnType { get; set; }
        public List<ParameterNode> Parameters { get; set; } = new();
        public List<StatementNode> Body { get; set; } = new();
        public Visibility Visibility { get; set; }
    }

    public enum MethodKind
    {
        Procedure,
        Function,
        Constructor,
        Destructor
    }

    public class ParameterNode : AstNode
    {
        public List<string> Names { get; set; } = new();
        public TypeNode? Type { get; set; }
        public ParameterModifier Modifier { get; set; }
    }

    public enum ParameterModifier
    {
        None,
        Const,
        Var,
        Out
    }

    // ========== TYPES ==========
    public abstract class TypeNode : AstNode
    {
        public string Name { get; set; } = "";
    }

    public class SimpleTypeNode : TypeNode
    {
        public bool IsBuiltIn { get; set; }
    }

    public class ArrayTypeNode : TypeNode
    {
        public TypeNode? ElementType { get; set; }
        public ExpressionNode? Size { get; set; }
    }

    public class ClassTypeNode : TypeNode
    {
        public ClassDeclarationNode? ClassDefinition { get; set; }
    }

    // ========== STATEMENTS ==========
    public abstract class StatementNode : AstNode { }

    public class CompoundStatementNode : StatementNode
    {
        public List<StatementNode> Statements { get; set; } = new();
    }

    public class AssignmentStatementNode : StatementNode
    {
        public ExpressionNode? Target { get; set; }
        public ExpressionNode? Value { get; set; }
    }

    public class IfStatementNode : StatementNode
    {
        public ExpressionNode? Condition { get; set; }
        public StatementNode? ThenBranch { get; set; }
        public StatementNode? ElseBranch { get; set; }
    }

    public class ForStatementNode : StatementNode
    {
        public string Variable { get; set; } = "";
        public ExpressionNode? Start { get; set; }
        public ExpressionNode? End { get; set; }
        public StatementNode? Body { get; set; }
    }

    public class WhileStatementNode : StatementNode
    {
        public ExpressionNode? Condition { get; set; }
        public StatementNode? Body { get; set; }
    }

    public class WithStatementNode : StatementNode
    {
        public ExpressionNode? Target { get; set; }
        public StatementNode? Body { get; set; }
    }

    public class CallStatementNode : StatementNode
    {
        public ExpressionNode? Call { get; set; }
    }

    // ========== EXPRESSIONS ==========
    public abstract class ExpressionNode : AstNode { }

    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; set; } = "";
    }

    public class MemberAccessNode : ExpressionNode
    {
        public ExpressionNode? Target { get; set; }
        public string Member { get; set; } = "";
    }

    public class CallExpressionNode : ExpressionNode
    {
        public ExpressionNode? Target { get; set; }
        public List<ExpressionNode> Arguments { get; set; } = new();
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode? Left { get; set; }
        public string Operator { get; set; } = "";
        public ExpressionNode? Right { get; set; }
    }

    public class LiteralNode : ExpressionNode
    {
        public object Value { get; set; } = "";
        public string LiteralType { get; set; } = "string";
    }

    public class UnaryExpressionNode : ExpressionNode
    {
        public string Operator { get; set; } = "";
        public ExpressionNode? Operand { get; set; }
    }

    // ========== OTHER ==========
    public class VariableDeclarationNode : AstNode
    {
        public List<string> Names { get; set; } = new();
        public TypeNode? Type { get; set; }
    }

    public class ConstantDeclarationNode : AstNode
    {
        public string Name { get; set; } = "";
        public TypeNode? Type { get; set; }
        public ExpressionNode? Value { get; set; }
    }
    
    // Helper classes for procedures and functions
    public class ProcedureNode : AstNode
    {
        public string Name { get; set; } = "";
        public List<ParameterNode> Parameters { get; set; } = new();
        public List<StatementNode> Body { get; set; } = new();
    }

    public class FunctionNode : AstNode
    {
        public string Name { get; set; } = "";
        public TypeNode? ReturnType { get; set; }
        public List<ParameterNode> Parameters { get; set; } = new();
        public List<StatementNode> Body { get; set; } = new();
    }
}
