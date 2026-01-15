using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DelphiToCsConverter
{
    // Exact match for your provided JSON structure
    public class DelphiUnit
    {
        public string Name { get; set; }
        public DelphiSection InterfaceSection { get; set; }
        public DelphiSection ImplementationSection { get; set; }
    }

    public class DelphiSection
    {
        public List<DelphiClass> Classes { get; set; } = new();
        public List<DelphiProcedure> Procedures { get; set; } = new();
    }

    public class DelphiClass
    {
        public string Name { get; set; }
        public List<DelphiField> Fields { get; set; } = new(); // Private fields
        public List<DelphiProperty> Properties { get; set; } = new(); // Public properties
    }

    public class DelphiField { public List<string> Names { get; set; } public string TypeName { get; set; } }
    public class DelphiProperty { public string Name { get; set; } public string TypeName { get; set; } }

    public class DelphiProcedure
    {
        public string Name { get; set; }
        public List<DelphiParam> Parameters { get; set; } = new();
        // Body contains polymorphic items ("$type": "assignment", etc.)
        public List<JsonElement> Body { get; set; } = new(); 
    }

    public class DelphiParam { public List<string> Names { get; set; } public string TypeName { get; set; } }
}