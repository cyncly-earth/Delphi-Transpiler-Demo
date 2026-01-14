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
        private readonly IReadOnlyDictionary<string, SemanticType> _types;

        public IrGenerator(IReadOnlyDictionary<string, SemanticType> types)
        {
            _types = types;
        }

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
                var semanticType = _types.TryGetValue(param.Value, out var type) ? type : new NamedType { QualifiedName = param.Value };
                var typeIr = InferType(semanticType);

                Console.WriteLine(
                    $"  - {param.Key} : {FormatType(semanticType)} -> {typeIr.GetType().Name}");

                ir.Parameters.Add(new ParameterIR
                {
                    Name = param.Key,
                    Type = typeIr
                });
            }

            // -----------------------------
            // 2. Effects
            // -----------------------------
            Console.WriteLine("[IRGEN] Effects:");

            if (!proc.Writes.Any() && !proc.Reads.Any() && !proc.Creates.Any() && !proc.Calls.Any())
            {
                Console.WriteLine("  (none)");
            }
            else
            {
                foreach (var write in proc.Writes)
                    Console.WriteLine($"  - Write:{write}");
                foreach (var read in proc.Reads)
                    Console.WriteLine($"  - Read:{read}");
                foreach (var create in proc.Creates)
                    Console.WriteLine($"  - Create:{create}");
                foreach (var call in proc.Calls)
                    Console.WriteLine($"  - Call:{call}");
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
            if (proc.Writes.Any(w => w.Contains("mtContact")))
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

                case ClassType @class:
                    return new ClassTypeIR
                    {
                        Name = @class.Name,
                        Fields = @class.Fields
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
