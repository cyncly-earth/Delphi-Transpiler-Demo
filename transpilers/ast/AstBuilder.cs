using DelphiTranspilerDemo.Ast;
namespace DelphiTranspilerDemo{
public sealed class AstBuilder : DelphiBaseListener
{
    public UnitNode? Unit { get; private set; }

    private ClassNode? _currentClass;
    private Visibility _currentVisibility = Visibility.Public;

    public override void EnterUnit(DelphiParser.UnitContext context)
{
    var unitName = context.unitHead()
                          .namespaceName()
                          .GetText();

    Unit = new UnitNode(unitName);
}
    public override void ExitClassDecl(DelphiParser.ClassDeclContext context)
    {
        _currentClass = null;
    }
    public override void EnterVisibility(DelphiParser.VisibilityContext context)
{
    _currentVisibility = context.GetText().ToLower() switch
    {
        "private" => Visibility.Private,
        "public" => Visibility.Public,
        "protected" => Visibility.Protected,
        _ => Visibility.Public
    };
}


public override void EnterClassDecl(DelphiParser.ClassDeclContext context)
{
    var className =
        context.Parent?.GetChild(0)?.GetText() ?? "UnknownClass";

    _currentClass = new ClassNode(className);
    Unit?.Classes.Add(_currentClass);
}
public override void EnterClassField(DelphiParser.ClassFieldContext context)
{
    if (_currentClass == null) return;

    var typeName = context.typeDecl().GetText();
    var type = new TypeRef(typeName);

    foreach (var ident in context.identList().ident())
    {
        _currentClass.Fields.Add(
            new FieldNode(
                ident.GetText(),
                type,
                _currentVisibility
            )
        );
    }
}
public override void EnterClassProperty(DelphiParser.ClassPropertyContext context)
{
    if (_currentClass == null) return;

    var name = context.ident().GetText();
    var type = new TypeRef(context.genericTypeIdent().GetText());

    _currentClass.Properties.Add(
        new PropertyNode(name, type, _currentVisibility)
    );
}
public override void EnterClassMethod(DelphiParser.ClassMethodContext context)
{
    // 1. Ignore anything outside class declaration
    if (_currentClass == null)
        return;

    // 2. methodKey is mandatory — if missing, ignore
    var methodKey = context.methodKey();
    if (methodKey == null)
        return;

    // 3. Method name may be missing in some grammar paths
    var ident = context.ident();
    if (ident == null)
        return;

    var methodKind = methodKey.GetText();
    var name = ident.GetText();

    TypeRef? returnType = null;
    if (context.typeDecl() != null)
        returnType = new TypeRef(context.typeDecl().GetText());

    _currentClass.Methods.Add(
        new MethodNode(
            name,
            methodKind,
            _currentVisibility,
            returnType
        )
    );
}
public sealed class UnitNode : AstNode
{
    public string Name { get; }
    public List<string> Uses { get; } = new();
    public List<ClassNode> Classes { get; } = new();
    public List<MethodNode> Procedures { get; } = new(); // NEW

    public UnitNode(string name) : base("Unit")
    {
        Name = name;
    }
}


}
}