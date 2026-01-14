using System;
using System.Collections.Generic;
using System.Linq;
using Transpiler.AST;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics.SemanticModels;
using DelphiTranspiler.Semantics.AstNodes;

namespace Transpiler.Semantics
{
    /// <summary>
    /// Converts syntax AST into semantic meaning.
    /// </summary>
    public sealed class SemanticEnrichmentPrototype
    {
        // =====================================================
        // STATE
        // =====================================================

        private readonly List<AstUnit> _loadedUnits = new();
        private readonly List<SemanticProcedure> _procedures = new();
        private readonly Dictionary<string, SemanticType> _types = new();

        // =====================================================
        // STAGE 1: Load AST Units
        // =====================================================

        public void LoadUnit(AstUnit unit)
        {
            _loadedUnits.Add(unit);
        }

        // =====================================================
        // STAGE 2: Collect Types
        // =====================================================

        public void CollectTypes()
        {
            foreach (var unit in _loadedUnits)
            {
                foreach (var cls in unit.Classes)
                {
                    var type = new ClassType
                    {
                        Name = $"{unit.Name}.{cls.Name}"
                    };

                    foreach (var field in cls.Fields)
                    {
                        type.Fields[field.Name] = field.Type;
                    }

                    _types[type.Name] = type;
                }
            }
        }

        // =====================================================
        // STAGE 3: Collect Procedures
        // =====================================================

        public void CollectProcedures()
        {
            foreach (var unit in _loadedUnits)
            {
                // Top-level procedures
                foreach (var proc in unit.Procedures)
                {
                    _procedures.Add(CreateSemanticProcedure(unit.Name, proc));
                }

                // Class methods
                foreach (var cls in unit.Classes)
                {
                    foreach (var proc in cls.Methods)
                    {
                        _procedures.Add(CreateSemanticProcedure(unit.Name, proc));
                    }
                }
            }
        }

        private SemanticProcedure CreateSemanticProcedure(
            string unitName,
            AstProcedure proc)
        {
            var semantic = new SemanticProcedure
            {
                Name = proc.Name,
                SourceUnit = unitName
            };

            ParseParameters(proc.Parameters, semantic);
            return semantic;
        }

        // =====================================================
        // STAGE 4: Resolve Parameter Types
        // =====================================================

        private void ParseParameters(
            string rawParams,
            SemanticProcedure semantic)
        {
            if (string.IsNullOrWhiteSpace(rawParams))
                return;

            var parts = rawParams.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var pair = part.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2) continue;

                var name = pair[0].Trim();
                var type = pair[1].Trim();

                // resolve class types
                var resolved =
                    _types.Keys.FirstOrDefault(t => t.EndsWith("." + type))
                    ?? type;

                semantic.Parameters[name] = resolved;
            }
        }

        // =====================================================
        // STAGE 5: Infer Effects (Reads / Writes)
        // =====================================================

        public void InferEffects()
        {
            foreach (var proc in _procedures)
            {
                var astProc = FindAstProcedure(proc);
                if (astProc == null || !astProc.HasBody)
                    continue;

                InferFromBody(proc, astProc.Body);
            }
        }

        private void InferFromBody(SemanticProcedure proc, string body)
{
    // --------------------
    // Writes
    // --------------------
    if (body.Contains("mtPerson"))
        proc.Writes.Add("Module.mtPerson");

    // --------------------
    // Reads
    // --------------------
    foreach (var param in proc.Parameters.Keys)
    {
        if (body.Contains(param + "."))
            proc.Reads.Add(param + ".*");
    }

    // --------------------
    // Creates (VERY IMPORTANT)
    // --------------------
    if (body.Contains("TPerson.Create"))
        proc.Creates.Add("TPerson");

    // --------------------
    // Calls
    // --------------------
    foreach (var other in _procedures)
    {
        if (other.Name != proc.Name &&
            body.Contains(other.Name + "("))
        {
            proc.Calls.Add(other.Name);
        }
    }

    // --------------------
    // UI classification
    // --------------------
    if (body.Contains("TEdit")
        || body.Contains("ShowMessage")
        || body.Contains("Click"))
    {
        proc.IsUiProcedure = true;
    }
}


        // =====================================================
        // STAGE 6: Helpers
        // =====================================================

        private AstProcedure? FindAstProcedure(SemanticProcedure proc)
        {
            foreach (var unit in _loadedUnits)
            {
                foreach (var p in unit.Procedures)
                    if (p.Name == proc.Name)
                        return p;

                foreach (var cls in unit.Classes)
                    foreach (var p in cls.Methods)
                        if (p.Name == proc.Name)
                            return p;
            }

            return null;
        }

        // =====================================================
        // OUTPUT
        // =====================================================

        public IReadOnlyList<SemanticProcedure> GetSemanticProcedures()
            => _procedures;

        public IReadOnlyDictionary<string, SemanticType> GetTypes()
            => _types;
    }
}
