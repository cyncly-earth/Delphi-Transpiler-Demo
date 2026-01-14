namespace DelphiTranspiler.Semantics.IR
{
    public class IrFunction
    {
        public string Name { get; set; } = "";
        public Dictionary<string, string> Parameters { get; set; } = new();
        public List<IrInstruction> Instructions { get; set; } = new();
    }
}
