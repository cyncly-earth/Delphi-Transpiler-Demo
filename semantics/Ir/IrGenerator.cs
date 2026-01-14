using System.Linq;
using DelphiTranspiler.Semantics.SemanticModels;
using DelphiTranspiler.Semantics.IR.ObjectGraph;

namespace DelphiTranspiler.Semantics
{
    /// <summary>
    /// Lowers a SemanticProcedure into a typed IR object graph.
    /// This IR is later used for code generation (C#, SQL, etc).
    /// </summary>
    public class IrGenerator
    {
        public ProcedureIR Generate(SemanticProcedure proc)
        {
            var ir = new ProcedureIR
            {
                Name = proc.Symbol
            };

            // -----------------------------
            // 1. Parameters
            // -----------------------------
            foreach (var param in proc.Parameters)
            {
                ir.Parameters.Add(new ParameterIR
                {
                    Name = param.Key,
                    Type = InferType(param.Value)
                });
            }

            // -----------------------------
            // 2. Person table logic
            // -----------------------------
            ir.Body.Add(new CallIR
            {
                Target = "Module.mtPerson.Open"
            });

            ir.Body.Add(new CallIR
            {
                Target = "Module.mtPerson.Append"
            });

            ir.Body.Add(new CallIR
            {
                Target = "PersonFields",
                Arguments = { "Person" }
            });

            ir.Body.Add(new CallIR
            {
                Target = "Module.mtPerson.Post"
            });

            // -----------------------------
            // 3. Effect-driven Contact logic
            // -----------------------------
            if (proc.Effects.Any(e => e.Contains("mtContact")))
            {
                var loop = new LoopIR
                {
                    Iterator = "Contacts"
                };

                loop.Body.Add(new CallIR
                {
                    Target = "Module.mtContact.Append"
                });

                loop.Body.Add(new CallIR
                {
                    Target = "ContactFields"
                });

                loop.Body.Add(new CallIR
                {
                    Target = "Module.mtContact.Post"
                });

                ir.Body.Add(loop);
            }

            // -----------------------------
            // 4. Return
            // -----------------------------
            ir.Body.Add(new ReturnIR());

            return ir;
        }

        // -----------------------------
        // Type inference helper
        // -----------------------------
        private TypeIR InferType(string semanticType)
        {
            // Example: Array<Contact>
            if (semanticType.StartsWith("Array<"))
            {
                var innerType = semanticType
                    .Replace("Array<", "")
                    .Replace(">", "");

                return new ArrayTypeIR
                {
                    ElementType = new NamedTypeIR
                    {
                        Name = innerType
                    }
                };
            }

            // Default: named type
            return new NamedTypeIR
            {
                Name = semanticType
            };
        }
    }
}
