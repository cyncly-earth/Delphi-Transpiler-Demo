namespace DelphiTranspiler.Semantics.IR
{
    public class IrInstruction
    {
        public string Op { get; set; } = "";
        public string? Target { get; set; }
        public List<string>? Args { get; set; }
        public string? Dest { get; set; }
        public string? Label { get; set; }
    }
}
