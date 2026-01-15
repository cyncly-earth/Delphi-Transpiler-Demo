public sealed class BackendIr
{
    public List<BackendProcedure> Procedures { get; } = new();
}

public sealed class BackendProcedure
{
    public string Name { get; set; }
    public List<BackendParam> Params { get; } = new();
    public List<string> Actions { get; } = new();
}

public sealed class BackendParam
{
    public string Name { get; set; }
    public string Type { get; set; }
}
