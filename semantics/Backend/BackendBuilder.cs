using System.Linq;
using DelphiTranspiler.Semantics.SemanticModels;

public static class BackendBuilder
{
    public static BackendIr BuildBackendIr(
        IReadOnlyList<SemanticProcedure> procedures)
    {
        var ir = new BackendIr();

        foreach (var proc in procedures.Where(p => !p.IsUiProcedure))
    {
        // ❌ Skip constructors / ToString
        if (!proc.Writes.Any())
            continue;

        var backendProc = new BackendProcedure
        {
            Name = proc.Name
        };

        foreach (var param in proc.Parameters)
        {
            backendProc.Params.Add(new BackendParam
            {
                Name = param.Key,
                Type = param.Value
            });
        }

        // Map semantic effects to actions
        if (proc.Writes.Contains("Module.mtPerson"))
        {
            backendProc.Actions.Add("open mtPerson");
            backendProc.Actions.Add("append row");
            backendProc.Actions.Add("write Person fields");
            backendProc.Actions.Add("commit");
        }

        ir.Procedures.Add(backendProc);
    }

        return ir;
    }
}
