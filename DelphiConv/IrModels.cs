using System.Collections.Generic;

namespace DelphiToCsConverter
{
    // This is the "Clean" data structure that both FE and BE will share
    public class SemanticSolution
    {
        public List<IrEntity> Entities { get; set; } = new();
        public List<IrService> Services { get; set; } = new();
        public List<IrLogic> LogicBlocks { get; set; } = new();
    }

    public class IrEntity
    {
        public string Name { get; set; }
        public List<IrProp> Properties { get; set; } = new();
    }

    public class IrProp 
    { 
        public string Name { get; set; } 
        public string Type { get; set; } 
        public bool IsKey { get; set; } 
    }

    public class IrService
    {
        public string Name { get; set; }
        public List<IrMethod> Methods { get; set; } = new();
    }

    public class IrMethod { public string Name { get; set; } public string Signature { get; set; } }

    public class IrLogic
    {
        public string Name { get; set; }
        public List<string> Lines { get; set; } = new();
    }
}