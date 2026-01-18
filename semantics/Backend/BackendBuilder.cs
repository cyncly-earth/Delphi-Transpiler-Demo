using System.Collections.Generic;
using System.Linq;
using DelphiTranspiler.Semantics.SemanticModels;

namespace DelphiTranspiler.Semantics
{
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
                    foreach(var p in proc.Parameters) 
                        bp.Params.Add(new BackendParam { Name = p.Key, Type = p.Value });
                    
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
