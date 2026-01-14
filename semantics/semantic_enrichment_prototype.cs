using System.Collections.Generic;
using DelphiTranspiler.Semantics.AstNodes;
using DelphiTranspiler.Semantics.SemanticModels;

namespace DelphiTranspiler.Semantics
{
    public class SemanticEnrichmentPrototype
    {
        // -----------------------------
        // INPUT: AST OBJECT GRAPH
        // -----------------------------
        private readonly List<AstNode> _astUnits = new();

        // -----------------------------
        // OUTPUT: SEMANTIC AST
        // -----------------------------
        private readonly Dictionary<string, SemanticType> _types = new();
        private readonly Dictionary<string, SemanticProcedure> _procedures = new();

        // -----------------------------
        // PHASE 1: Load AST nodes
        // -----------------------------
        public void LoadUnit(AstNode ast)
        {
            Console.WriteLine($"Loaded: {ast.GetType().Name}");
            _astUnits.Add(ast);
        }

        // -----------------------------
        // PHASE 2: Collect Types
        // -----------------------------
        public void CollectTypes()
        {
            foreach (var node in _astUnits)
            {
                if (node is ClassDeclNode classNode)
                {
                    var type = new SemanticType
                    {
                        Name = classNode.Name
                    };

                    foreach (var field in classNode.Fields)
                    {
                        type.Fields[field] = "unknown";
                    }

                    _types[type.Name] = type;
                }
            }
        }

        // -----------------------------
        // PHASE 3: Collect Procedures
        // -----------------------------
        public void CollectProcedures()
        {
            foreach (var node in _astUnits)
            {
                if (node is ProcedureDeclNode procNode)
                {
                    var proc = new SemanticProcedure
                    {
                        Symbol = procNode.Name
                    };

                    foreach (var p in procNode.Params)
                    {
                        proc.Parameters[p] = "unresolved";
                    }

                    _procedures[proc.Symbol] = proc;
                }
            }
        }

        // -----------------------------
        // PHASE 4: Resolve Types (rules)
        // -----------------------------
        public void ResolveProcedureTypes()
        {
            if (_procedures.TryGetValue("AddPerson", out var proc))
            {
                proc.Parameters["Person"] = "classPerson.TPerson";
                proc.Parameters["Contacts"] = "Array<classContact.TContact>";
            }
        }

        // -----------------------------
        // PHASE 5: Infer Side Effects
        // -----------------------------
        public void InferEffects()
        {
            if (_procedures.TryGetValue("AddPerson", out var proc))
            {
                proc.Effects.Add("write:Module.mtPerson");
                proc.Effects.Add("write:Module.mtContact");
            }
        }

        // -----------------------------
        // OUTPUT ACCESSORS
        // -----------------------------
        public IEnumerable<SemanticType> GetSemanticTypes() => _types.Values;
        public IEnumerable<SemanticProcedure> GetSemanticProcedures() => _procedures.Values;
    }
}
