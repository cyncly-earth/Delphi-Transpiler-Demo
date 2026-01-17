using System.Collections.Generic;
using System.Linq;

namespace DelphiTranspiler.Semantics.SemanticModels
{
    // ==========================================
    // 1. SEMANTIC MODELS
    // ==========================================
    public abstract class SemanticType { }

    public class NamedType : SemanticType { public string QualifiedName { get; set; } = ""; }
    public class ArrayType : SemanticType { public SemanticType ElementType { get; set; } = new UnresolvedType(); }
    public class UnresolvedType : SemanticType { }
    public class ClassType : SemanticType { 
        public string Name { get; set; } = ""; 
        public Dictionary<string, string> Fields { get; } = new(); 
    }

    public class SemanticProcedure {
        public string Name { get; set; } = "";
        public string SourceUnit { get; set; } = "";
        public Dictionary<string, string> Parameters { get; } = new();
        public List<string> Reads { get; } = new();
        public List<string> Writes { get; } = new();
        public List<string> Creates { get; } = new();
        public List<string> Calls { get; } = new();
        public bool IsUiProcedure { get; set; } = false;
    }

    public class BackendIr { public List<BackendProcedure> Procedures { get; set; } = new(); }
    public class BackendProcedure { 
        public string Name { get; set; } = ""; 
        public List<BackendParam> Params { get; set; } = new(); 
        public List<string> Actions { get; set; } = new(); 
    }
    public class BackendParam { public string Name { get; set; } = ""; public object Type { get; set; } = ""; }

    public sealed class EntityModel { public List<EntityDefinition> Entities { get; } = new(); }
    public sealed class EntityDefinition { public string Name { get; set; } = ""; public List<EntityField> Fields { get; } = new(); }
    public sealed class EntityField { public string Name { get; set; } = ""; public string Type { get; set; } = ""; }
}

namespace DelphiTranspiler.Semantics
{
    using DelphiTranspiler.Semantics.SemanticModels;

    // ==========================================
    // 2. ENTITY MODEL BUILDER
    // ==========================================
    public static class EntityModelBuilder
    {
        public static EntityModel BuildEntityModel(IReadOnlyDictionary<string, SemanticType> types)
        {
            var model = new EntityModel();
            foreach (var type in types.Values)
            {
                if (!(type is ClassType classType)) continue;
                if (classType.Name.Contains("PersonView")) continue;

                var entity = new EntityDefinition { Name = classType.Name };
                foreach (var field in classType.Fields)
                {
                    entity.Fields.Add(new EntityField { 
                        Name = (field.Key.StartsWith("c") && field.Key.Length > 1) ? field.Key.Substring(1).ToLower() : field.Key.ToLower(),
                        Type = field.Value == "Integer" ? "int" : "string"
                    });
                }
                model.Entities.Add(entity);
            }
            return model;
        }
    }

    // ==========================================
    // 3. BACKEND BUILDER
    // ==========================================
    public static class BackendBuilder
    {
        public static BackendIr BuildBackendIr(IReadOnlyList<SemanticProcedure> procedures)
        {
            var ir = new BackendIr();
            foreach (var proc in procedures)
            {
                if (!proc.IsUiProcedure && proc.Writes.Any())
                {
                    var bp = new BackendProcedure { Name = proc.Name };
                    foreach(var p in proc.Parameters) bp.Params.Add(new BackendParam { Name = p.Key, Type = p.Value });
                    if(proc.Writes.Contains("Module.mtPerson")) {
                        bp.Actions.Add("open mtPerson");
                        bp.Actions.Add("commit");
                    }
                    ir.Procedures.Add(bp);
                }
            }
            return ir;
        }
    }
}
