public class UiIrRoot
{
    public List<UiAction> UiActions { get; set; } = new();
}

public class UiAction
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public UiForm Form { get; set; } = new();
    public BackendCall BackendCall { get; set; } = new();
}

public class UiForm
{
    public string Entity { get; set; } = string.Empty;
    public List<UiField> Fields { get; set; } = new();
}

public class UiField
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class BackendCall
{
    public string Procedure { get; set; } = string.Empty;
    public List<BackendArgument> Arguments { get; set; } = new();
}

public class BackendArgument
{
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
