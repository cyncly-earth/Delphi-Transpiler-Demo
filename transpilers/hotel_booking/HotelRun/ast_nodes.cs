public class FormNode
{
    public string Name;
    public List<FieldNode> Fields = new();
    public List<ButtonNode> Buttons = new();
}

public class FieldNode
{
    public string Name;
}

public class ButtonNode
{
    public string Name;
}
