using DelphiTranspiler.Semantics.SemanticModels;
using DelphiTranspiler.Semantics.IR;

namespace DelphiTranspiler.Semantics
{
    public class IrGenerator
    {
        public IrFunction Generate(SemanticProcedure proc)
        {
            var ir = new IrFunction
            {
                Name = proc.Symbol,
                Parameters = proc.Parameters
            };

            // Fixed lowering for AddPerson (prototype)
            ir.Instructions.Add(new IrInstruction
            {
                Op = "call",
                Target = "Module.mtPerson.Open"
            });

            ir.Instructions.Add(new IrInstruction
            {
                Op = "call",
                Target = "Module.mtPerson.Append"
            });

            ir.Instructions.Add(new IrInstruction
            {
                Op = "call",
                Target = "PersonFields",
                Args = new() { "Person" }
            });

            ir.Instructions.Add(new IrInstruction
            {
                Op = "call",
                Target = "Module.mtPerson.Post"
            });

            // Effects guide further lowering
            if (proc.Effects.Any(e => e.Contains("mtContact")))
            {
                ir.Instructions.Add(new IrInstruction
                {
                    Op = "loop",
                    Dest = "Contacts"
                });

                ir.Instructions.Add(new IrInstruction
                {
                    Op = "call",
                    Target = "Module.mtContact.Append"
                });

                ir.Instructions.Add(new IrInstruction
                {
                    Op = "call",
                    Target = "ContactFields"
                });

                ir.Instructions.Add(new IrInstruction
                {
                    Op = "call",
                    Target = "Module.mtContact.Post"
                });

                ir.Instructions.Add(new IrInstruction
                {
                    Op = "endloop"
                });
            }

            ir.Instructions.Add(new IrInstruction { Op = "return" });

            return ir;
        }
    }
}
