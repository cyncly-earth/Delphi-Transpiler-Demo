using System.Collections.Generic;

namespace DelphiTranspiler.CodeGen.Models
{
    public class UiIrRoot
    {
        public List<UiAction> UiActions { get; set; } = new();
    }

    public sealed class UiModel
    {
        public List<UiAction> UiActions { get; set; } = new();
    }

    public sealed class UiAction
    {
        public string Name { get; set; } = "";
        public string Kind { get; set; } = "";
        public UiForm Form { get; set; } = new();
        public UiBackendCall BackendCall { get; set; } = new();
    }

    public sealed class UiForm
    {
        public string Entity { get; set; } = "";
        public List<UiField> Fields { get; set; } = new();
    }

    public sealed class UiField
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public sealed class UiBackendCall
    {
        public string Procedure { get; set; } = "";
        public List<UiArgument> Arguments { get; set; } = new();
    }

    public sealed class UiArgument
    {
        public string Type { get; set; } = "";
        public string Source { get; set; } = "";
    }
}