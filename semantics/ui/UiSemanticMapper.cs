using System.Collections.Generic;
using System.Linq;
using DelphiTranspiler.CodeGen.Models;       // <--- Uses the correct UiModel/UiAction
using DelphiTranspiler.Semantics.SemanticModels; 

namespace DelphiTranspiler.Semantics.Ui
{
    public static class UiSemanticMapper
    {
        public static UiModel BuildUiModel(
            IReadOnlyList<SemanticProcedure> procedures,
            IReadOnlyDictionary<string, SemanticType> types)
        {
            var model = new UiModel();

            foreach (var proc in procedures.Where(p => p.IsUiProcedure))
            {
                model.UiActions.Add(BuildUiAction(proc, types));
            }
            return model;
        }

        private static UiAction BuildUiAction(SemanticProcedure uiProc, IReadOnlyDictionary<string, SemanticType> types)
        {
            string createdEntity = uiProc.Creates.FirstOrDefault() ?? "Unknown";
            
            var typeKey = types.Keys.FirstOrDefault(k => k.EndsWith("." + createdEntity)) 
                          ?? types.Keys.FirstOrDefault(k => k.Contains(createdEntity));

            var form = new UiForm();
            if (typeKey != null && types[typeKey] is ClassType ct)
            {
                form = BuildForm(ct);
            }

            return new UiAction
            {
                Name = uiProc.Name,
                Kind = "form-submit",
                Form = form,
                BackendCall = new UiBackendCall 
                { 
                    Procedure = uiProc.Calls.FirstOrDefault(c => !c.Contains("Create")) ?? "Unknown",
                    Arguments = { new UiArgument { Type = createdEntity, Source = "form" } }
                }
            };
        }

        private static UiForm BuildForm(ClassType type)
        {
            var form = new UiForm { Entity = type.Name };
            foreach (var field in type.Fields.Keys)
            {
                if (field == "cID" || field == "cClient") continue;
                form.Fields.Add(new UiField
                {
                    Name = (field.StartsWith("c") && field.Length > 1) ? field.Substring(1).ToLower() : field.ToLower(),
                    Type = "string"
                });
            }
            return form;
        }
    }
}