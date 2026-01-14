using DelphiTranspiler.Semantics.SemanticModels;

public static class UiSemanticMapper
{
    public static UiModel BuildUiModel(
        IReadOnlyList<SemanticProcedure> procedures,
        IReadOnlyDictionary<string, SemanticType> types)
    {
        var model = new UiModel();

        foreach (var proc in procedures.Where(p => p.IsUiProcedure))
        {
            model.UiActions.Add(
                BuildUiAction(proc, types)
            );
        }

        return model;
    }

    private static UiAction BuildUiAction(
        SemanticProcedure uiProc,
        IReadOnlyDictionary<string, SemanticType> types)
    {
        // 1️⃣ What entity is created
        var createdEntity = uiProc.Creates.First(); // TPerson

        var typeKey =
            types.Keys.Single(k => k.EndsWith(createdEntity));

        var semanticType = types[typeKey];

        // 2️⃣ Build form
        var form = BuildForm((ClassType)semanticType);

        // 3️⃣ Backend call
        var backendCall = BuildBackendCall(uiProc, typeKey);

        return new UiAction
        {
            Name = uiProc.Name,
            Kind = "form-submit",
            Form = form,
            BackendCall = backendCall
        };
    }

    private static UiForm BuildForm(ClassType type)
    {
        var form = new UiForm
        {
            Entity = type.Name
        };

        foreach (var field in type.Fields.Keys)
        {
            // Ignore system fields
            if (field is "cID" or "cClient")
                continue;

            form.Fields.Add(new UiField
            {
                Name = Normalize(field),
                Type = "string"
            });
        }

        return form;
    }

    private static string Normalize(string delphiField)
    {
        // cFirst → first
        if (delphiField.StartsWith("c") && delphiField.Length > 1)
            return delphiField.Substring(1).ToLower();

        return delphiField.ToLower();
    }

    private static UiBackendCall BuildBackendCall(
        SemanticProcedure proc,
        string entityType)
    {
        return new UiBackendCall
        {
            Procedure = proc.Calls.First(c => c != "Create"), // Skip Create, call the main procedure
            Arguments =
            {
                new UiArgument
                {
                    Type = entityType,
                    Source = "form"
                }
            }
        };
    }
}
