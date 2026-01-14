using System;
using System.Linq;
using DelphiTranspiler.Semantics.SemanticModels;
using DelphiTranspiler.Semantics.IR.ObjectGraph;

namespace DelphiTranspiler.Semantics
{
    /// <summary>
    /// Lowers a SemanticProcedure into a typed IR object graph.
    /// </summary>
    public sealed class IrGenerator
    {
        public ProcedureIR Generate(SemanticProcedure proc)
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"[IRGEN] Processing procedure: {proc.Name}");

            var ir = new ProcedureIR
            {
                Name = proc.Name
            };

            // -----------------------------
            // 1. Parameters
            // -----------------------------
            Console.WriteLine("[IRGEN] Parameters:");

            if (!proc.Parameters.Any())
            {
                Console.WriteLine("  (none)");
            }

            foreach (var param in proc.Parameters)
            {
                var typeIr = InferType(param.Type);

                Console.WriteLine(
                    $"  - {param.Name} : {FormatType(param.Type)} -> {typeIr.GetType().Name}");

                ir.Parameters.Add(new ParameterIR
                {
                    Name = param.Name,
                    Type = typeIr
                });
            }

            // -----------------------------
            // 2. Effects
            // -----------------------------
            Console.WriteLine("[IRGEN] Effects:");

            if (!proc.Effects.Any())
            {
                Console.WriteLine("  (none)");
            }

            foreach (var effect in proc.Effects)
            {
                Console.WriteLine($"  - {effect.Kind}:{effect.Target}");
            }

            // -----------------------------
            // 3. Person table logic (prototype)
            // -----------------------------
            Console.WriteLine("[IRGEN] Emitting Person table IR");

            ir.Body.Add(new CallIR { Target = "Module.mtPerson.Open" });
            ir.Body.Add(new CallIR { Target = "Module.mtPerson.Append" });
            ir.Body.Add(new CallIR
            {
                Target = "PersonFields",
                Arguments = { "Person" }
            });
            ir.Body.Add(new CallIR { Target = "Module.mtPerson.Post" });

            // -----------------------------
            // 4. Effect-driven Contact logic
            // -----------------------------
            if (proc.Effects.Any(e =>
                    e.Kind == EffectKind.Write &&
                    e.Target.Contains("mtContact")))
            {
                Console.WriteLine("[IRGEN] Detected mtContact write → emitting contact loop");

                var loop = new LoopIR
                {
                    Iterator = "Contacts"
                };

                loop.Body.Add(new CallIR { Target = "Module.mtContact.Append" });
                loop.Body.Add(new CallIR { Target = "ContactFields" });
                loop.Body.Add(new CallIR { Target = "Module.mtContact.Post" });

                ir.Body.Add(loop);
            }
            else
            {
                Console.WriteLine("[IRGEN] No mtContact effect → skipping contact loop");
            }

            // -----------------------------
            // 5. Return
            // -----------------------------
            ir.Body.Add(new ReturnIR());

            Console.WriteLine("[IRGEN] IR generation completed");
            Console.WriteLine($"[IRGEN] IR Body instruction count: {ir.Body.Count}");
            Console.WriteLine("========================================");

            return ir;
        }

        // =====================================================
        // Type inference (SemanticType → TypeIR)
        // =====================================================

        private TypeIR InferType(SemanticType semanticType)
        {
            Console.WriteLine($"[IRGEN] Inferring type for: {FormatType(semanticType)}");

            switch (semanticType)
            {
                case ArrayType array:
                    return new ArrayTypeIR
                    {
                        ElementType = InferType(array.ElementType)
                    };

                case NamedType named:
                    return new NamedTypeIR
                    {
                        Name = named.QualifiedName
                    };

                case UnresolvedType:
                    return new NamedTypeIR
                    {
                        Name = "object"
                    };

                default:
                    throw new NotSupportedException(
                        $"Unsupported SemanticType: {semanticType.GetType().Name}");
            }
        }

        private static string FormatType(SemanticType type)
        {
            return type switch
            {
                NamedType n => n.QualifiedName,
                ArrayType a => $"Array<{FormatType(a.ElementType)}>",
                UnresolvedType => "unresolved",
                _ => "unknown"
            };
        }
    }
}
