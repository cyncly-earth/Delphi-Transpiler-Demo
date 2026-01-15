public sealed class SemanticProcedure
{
    public string Name { get; set; } = string.Empty;
 
   
    public Dictionary<string, string> Parameters { get; } = new();
 
    public HashSet<string> Reads { get; } = new();
    public HashSet<string> Writes { get; } = new();
 
   
    public HashSet<string> Creates { get; } = new();
    public HashSet<string> Calls { get; } = new();
 
    public bool IsUiProcedure { get; set; }
    public string SourceUnit { get; set; } = string.Empty;
}
 