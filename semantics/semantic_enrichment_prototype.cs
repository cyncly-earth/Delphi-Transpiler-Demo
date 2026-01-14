using System.Collections.Generic;
using System.Linq;
using DelphiTranspiler.Semantics.AstNodes;
using DelphiTranspiler.Semantics.SemanticModels;

namespace DelphiTranspiler.Semantics
{
    /// <summary>
    /// Performs semantic enrichment over AST nodes.
    /// Converts syntax-level AST into semantic procedures.
    /// </summary>
    public sealed class SemanticEnrichmentPrototype
    {
        private readonly List<AstNode> _loadedUnits = new();
        private readonly List<SemanticProcedure> _procedures = new();

        // =====================================================
        // STAGE 1: Load AST
        // =====================================================

        public void LoadUnit(AstNode ast)
        {
            _loadedUnits.Add(ast);
        }

        // =====================================================
        // STAGE 2: Collect Procedures
        // =====================================================

        public void CollectProcedures()
        {
            foreach (var ast in _loadedUnits)
            {
                if (ast is not ProcedureDeclNode procNode)
                    continue;

                var semanticProc = new SemanticProcedure
                {
                    Name = procNode.Name
                };

                // Parameters start unresolved
                foreach (var param in procNode.Params)
                {
                    semanticProc.Parameters.Add(new SemanticParameter
                    {
                        Name = param,
                        Type = SemanticType.Unresolved
                    });
                }

                _procedures.Add(semanticProc);
            }
        }

        // =====================================================
        // STAGE 3: Resolve Parameter Types
        // =====================================================

        public void ResolveProcedureTypes()
{
    foreach (var proc in _procedures)
    {
        foreach (var param in proc.Parameters)
        {
            if (param.Type is not UnresolvedType)
                continue;

            // -----------------------------
            // Primitive inference
            // -----------------------------
            if (param.Name is "Index" or "Count" or "Length")
            {
                param.Type = new NamedType { QualifiedName = "int" };
                continue;
            }

            if (param.Name.StartsWith("n"))
            {
                param.Type = new NamedType { QualifiedName = "string" };
                continue;
            }

            // -----------------------------
            // Domain object inference
            // -----------------------------
            if (param.Name == "Person")
            {
                param.Type = new NamedType
                {
                    QualifiedName = "classPerson.TPerson"
                };
                continue;
            }

            if (param.Name == "Contact")
            {
                param.Type = new NamedType
                {
                    QualifiedName = "classContact.TContact"
                };
                continue;
            }

            if (param.Name == "Contacts")
            {
                param.Type = new ArrayType
                {
                    ElementType = new NamedType
                    {
                        QualifiedName = "classContact.TContact"
                    }
                };
                continue;
            }

            // -----------------------------
            // Fallback (never leave unresolved)
            // -----------------------------
            param.Type = new NamedType { QualifiedName = "object" };
        }
    }
}


        // =====================================================
        // STAGE 4: Infer Effects
        // =====================================================

        public void InferEffects()
        {
            foreach (var proc in _procedures)
            {
                // Heuristic-based inference
                if (proc.Name.StartsWith("Add") ||
                    proc.Name.StartsWith("Create") ||
                    proc.Name.StartsWith("Save"))
                {
                    proc.Effects.Add(new SemanticEffect
                    {
                        Kind = EffectKind.Write,
                        Target = "Module.Unknown"
                    });
                }
            }
        }

        // =====================================================
        // OUTPUT
        // =====================================================

        public IReadOnlyList<SemanticProcedure> GetSemanticProcedures()
        {
            return _procedures;
        }

        // =====================================================
        // NOT USED YET (placeholders)
        // =====================================================

        public void CollectTypes()
        {
            // Will be implemented later
        }
    }
}
