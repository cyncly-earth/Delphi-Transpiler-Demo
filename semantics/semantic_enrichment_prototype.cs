using System;
using System.Collections.Generic;
using System.Linq;
using Transpiler.AST;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics.SemanticModels;
// Removed unused AstNodes import

namespace Transpiler.Semantics
{
    /// <summary>
    /// Converts syntax AST into semantic meaning.
    /// </summary>
    public sealed class SemanticEnrichmentPrototype
    {
        private readonly List<AstUnit> _loadedUnits = new();
        private readonly List<SemanticProcedure> _procedures = new();
        private readonly Dictionary<string, SemanticType> _types = new();

        public void LoadUnit(AstUnit unit)
        {
            _loadedUnits.Add(unit);
        }

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

        public void CollectProcedures()
        {
            foreach (var unit in _loadedUnits)
            {
                foreach (var proc in unit.Procedures)
                {
                    _procedures.Add(CreateSemanticProcedure(unit.Name, proc));
                }

                foreach (var cls in unit.Classes)
                {
                    foreach (var proc in cls.Methods)
                    {
                        _procedures.Add(CreateSemanticProcedure(unit.Name, proc));
                    }
                }
            }
        }

        private SemanticProcedure CreateSemanticProcedure(string unitName, AstProcedure proc)
        {
            var semantic = new SemanticProcedure
            {
                Name = proc.Name,
                SourceUnit = unitName
            };

            ParseParameters(proc.Parameters, semantic);
            return semantic;
        }

        private void ParseParameters(string rawParams, SemanticProcedure semantic)
        {
            if (string.IsNullOrWhiteSpace(rawParams)) return;

            var parts = rawParams.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var pair = part.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2) continue;

                var name = pair[0].Trim();
                var type = pair[1].Trim();

                var resolved = _types.Keys.FirstOrDefault(t => t.EndsWith("." + type)) ?? type;
                semantic.Parameters[name] = resolved;
            }
        }

        public void InferEffects()
        {
            foreach (var proc in _procedures)
            {
                var astProc = FindAstProcedure(proc);
                if (astProc == null || !astProc.HasBody) continue;
                InferFromBody(proc, astProc.Body);
            }
        }

        private void InferFromBody(SemanticProcedure proc, string body)
        {
            if (body.Contains("mtPerson")) proc.Writes.Add("Module.mtPerson");

            foreach (var param in proc.Parameters.Keys)
            {
                if (body.Contains(param + ".")) proc.Reads.Add(param + ".*");
            }

            if (body.Contains("TPerson.Create")) proc.Creates.Add("TPerson");

            foreach (var other in _procedures)
            {
                if (other.Name != proc.Name && body.Contains(other.Name + "("))
                {
                    proc.Calls.Add(other.Name);
                }
            }

            if (body.Contains("TEdit") || body.Contains("ShowMessage") || body.Contains("Click"))
            {
                proc.IsUiProcedure = true;
            }
        }

        private AstProcedure? FindAstProcedure(SemanticProcedure proc)
        {
            foreach (var unit in _loadedUnits)
            {
                foreach (var p in unit.Procedures)
                    if (p.Name == proc.Name) return p;

                foreach (var cls in unit.Classes)
                    foreach (var p in cls.Methods)
                        if (p.Name == proc.Name) return p;
            }
            return null;
        }

        public IReadOnlyList<SemanticProcedure> GetSemanticProcedures() => _procedures;
        public IReadOnlyDictionary<string, SemanticType> GetTypes() => _types;
    }
}