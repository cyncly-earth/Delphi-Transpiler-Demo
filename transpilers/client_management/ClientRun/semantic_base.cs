public class ScreenIR
{
    public string ScreenName;
    public List<FieldIR> Fields = new();
    public List<ActionIR> Actions = new();
}

public class FieldIR
{
    public string Name;
    public string Type;
}

public class ActionIR
{
    public string Name;
    public string Calls;
}
