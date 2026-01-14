public class UiIrRoot
{
    public List<UiAction> UiActions { get; set; }
}

public class UiAction
{
    public string Name { get; set; }
    public string Kind { get; set; }
    public UiForm Form { get; set; }
    public BackendCall BackendCall { get; set; }
}

public class UiForm
{
    public string Entity { get; set; }
    public List<UiField> Fields { get; set; }
}

public class UiField
{
    public string Name { get; set; }
    public string Type { get; set; }
}

public class BackendCall
{
    public string Procedure { get; set; }
    public List<BackendArgument> Arguments { get; set; }
}

public class BackendArgument
{
    public string Type { get; set; }
    public string Source { get; set; }
}
