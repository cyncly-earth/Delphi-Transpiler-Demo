using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.SemanticModels;
using Transpiler.Semantics;
using DelphiTranspiler.Semantics.Ui; // For UiSemanticMapper

public class SemanticEnrichmentRunner
{
    private readonly SemanticEnrichmentPrototype _enricher;

    public SemanticEnrichmentRunner(SemanticEnrichmentPrototype enricher)
    {
        _enricher = enricher;
    }

    public (string uiJson, string entityJson, string backendJson) ProcessFeature(IEnumerable<AstUnit> astUnits)
    {
        // 1. Load
        foreach (var ast in astUnits) _enricher.LoadUnit(ast);

        // 2. Enrich
        _enricher.CollectTypes();
        _enricher.CollectProcedures();
        _enricher.InferEffects();

        // 3. Generate IRs
        var uiModel = UiSemanticMapper.BuildUiModel(_enricher.GetSemanticProcedures(), _enricher.GetTypes());
        var uiJson = JsonSerializer.Serialize(uiModel, new JsonSerializerOptions { WriteIndented = true });

        var entityModel = EntityModelBuilder.BuildEntityModel(_enricher.GetTypes());
        var entityJson = JsonSerializer.Serialize(entityModel, new JsonSerializerOptions { WriteIndented = true });

        var backendIr = BackendBuilder.BuildBackendIr(_enricher.GetSemanticProcedures());
        var backendJson = JsonSerializer.Serialize(backendIr, new JsonSerializerOptions { WriteIndented = true });

        return (uiJson, entityJson, backendJson);
    }
}