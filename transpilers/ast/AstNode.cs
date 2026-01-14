namespace DelphiTranspilerDemo.Ast;
// This file defines the abstract syntax tree (AST) model used by the Delphi transpiler.
// The AST captures the structural elements of Delphi source code such as units, classes,
// fields, properties, and method signatures, enabling automated generation of modern
// .NET and Angular application scaffolding.

public abstract class AstNode
{
    public string Kind { get; }

    protected AstNode(string kind)
    {
        Kind = kind;
    }
}
public sealed class UnitNode : AstNode
{
    public string Name { get; }
    public List<string> Uses { get; } = new();
    public List<ClassNode> Classes { get; } = new();

    public UnitNode(string name) : base("Unit")
    {
        Name = name;
    }
}
public sealed class ClassNode : AstNode
{
    public string Name { get; }
    public List<FieldNode> Fields { get; } = new();
    public List<PropertyNode> Properties { get; } = new();
    public List<MethodNode> Methods { get; } = new();

    public ClassNode(string name) : base("Class")
    {
        Name = name;
    }
}
public enum Visibility
{
    Private,
    Public,
    Protected
}
public sealed class TypeRef
{
    public string Name { get; }

    public TypeRef(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
public sealed class FieldNode : AstNode
{
    public string Name { get; }
    public TypeRef Type { get; }
    public Visibility Visibility { get; }

    public FieldNode(string name, TypeRef type, Visibility visibility)
        : base("Field")
    {
        Name = name;
        Type = type;
        Visibility = visibility;
    }
}
public sealed class PropertyNode : AstNode
{
    public string Name { get; }
    public TypeRef Type { get; }
    public Visibility Visibility { get; }

    public PropertyNode(string name, TypeRef type, Visibility visibility)
        : base("Property")
    {
        Name = name;
        Type = type;
        Visibility = visibility;
    }
}
public sealed class MethodNode : AstNode
{
    public string Name { get; }
    public string Kind { get; } // constructor | destructor | function | procedure
    public Visibility Visibility { get; }
    public TypeRef? ReturnType { get; }
    public List<ParameterNode> Parameters { get; } = new();

    public MethodNode(
        string name,
        string kind,
        Visibility visibility,
        TypeRef? returnType = null)
        : base("Method")
    {
        Name = name;
        Kind = kind;
        Visibility = visibility;
        ReturnType = returnType;
    }
}
public sealed class ParameterNode
{
    public string Name { get; }
    public TypeRef Type { get; }

    public ParameterNode(string name, TypeRef type)
    {
        Name = name;
        Type = type;
    }
}

