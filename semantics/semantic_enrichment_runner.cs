using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;
using DelphiTranspiler.Semantics.SemanticModels;

using System.Text.Json;
using Transpiler.AST;
using Transpiler.Semantics;



public class SemanticEnrichmentRunner
{
    private readonly SemanticEnrichmentPrototype _enricher;

    public SemanticEnrichmentRunner(SemanticEnrichmentPrototype enricher)
    {
        _enricher = enricher;
    }

    public (string uiJson, string entityJson, string backendJson) ProcessFeature(IEnumerable<AstUnit> astUnits)
    {
        // =====================================================
        // STAGE 0: LOG AST INPUT
        // =====================================================
        LogAstInput(astUnits);

        // =====================================================
        // STAGE 1: Load ASTs
        // =====================================================
        foreach (var ast in astUnits)
        {
            _enricher.LoadUnit(ast);
        }

        // =====================================================
        // STAGE 2: Semantic enrichment
        // =====================================================
        _enricher.CollectTypes();
        _enricher.CollectProcedures();
        _enricher.InferEffects();

        // =====================================================
        // STAGE 3: LOG SEMANTIC AST
        // =====================================================
        Console.WriteLine("===== SEMANTIC AST =====");
Console.WriteLine();
Console.WriteLine("Types:");

foreach (var t in _enricher.GetTypes())
{
    Console.WriteLine($"  {t.Key}");
    if (t.Value is ClassType classType)
        Console.WriteLine($"    Fields: {string.Join(", ", classType.Fields.Keys)}");
}
Console.WriteLine();
Console.WriteLine("Procedures:");

foreach (var proc in _enricher.GetSemanticProcedures())
{
    Console.WriteLine($"Procedure: {proc.Name}");

    if (proc.Parameters.Any())
        Console.WriteLine(
            $"  Params: {string.Join(", ",
                proc.Parameters.Select(p => $"{p.Key} : {p.Value}"))}");

    if (proc.Reads.Any())
        Console.WriteLine($"  Reads: {string.Join(", ", proc.Reads)}");

    if (proc.Writes.Any())
        Console.WriteLine($"  Writes: {string.Join(", ", proc.Writes)}");

    if (proc.Creates.Any())
        Console.WriteLine($"  Creates: {string.Join(", ", proc.Creates)}");

    if (proc.Calls.Any())
        Console.WriteLine($"  Calls: {string.Join(", ", proc.Calls)}");

    Console.WriteLine($"  Kind: {(proc.IsUiProcedure ? "UI" : "Backend")}");
    Console.WriteLine();
}


// =====================================================
// STAGE 4: UI IR GENERATION
// =====================================================

var uiModel = UiSemanticMapper.BuildUiModel(
    _enricher.GetSemanticProcedures(),
    _enricher.GetTypes()
);

var uiJson = JsonSerializer.Serialize(
    uiModel,
    new JsonSerializerOptions { WriteIndented = true }
);


// =====================================================
// STAGE 5: ENTITY MODEL IR GENERATION
// =====================================================

var entityModel = EntityModelBuilder.BuildEntityModel(
    _enricher.GetTypes()
);

var entityJson = JsonSerializer.Serialize(
    entityModel,
    new JsonSerializerOptions { WriteIndented = true }
);

Console.WriteLine("===== ENTITY MODEL IR =====");
Console.WriteLine(entityJson);

// =====================================================

var backendIr = BackendBuilder.BuildBackendIr(
    _enricher.GetSemanticProcedures()
);

var backendJson = JsonSerializer.Serialize(
    backendIr,
    new JsonSerializerOptions { WriteIndented = true }
);

Console.WriteLine("===== BACKEND IR =====");
Console.WriteLine(backendJson);

        return (uiJson, entityJson, backendJson);





    }

    // =====================================================
    // LOGGING HELPERS
    // =====================================================

    private void LogAstInput(IEnumerable<AstUnit> units)
{
    Console.WriteLine("===== AST INPUT =====");

    foreach (var unit in units)
    {
        Console.WriteLine($"Unit: {unit.Name}");

        // -----------------------------
        // Classes
        // -----------------------------
        if (unit.Classes.Any())
        {
            foreach (var cls in unit.Classes)
            {
                Console.WriteLine($"  Class: {cls.Name}");

                // Fields
                if (cls.Fields.Any())
                {
                    foreach (var field in cls.Fields)
                    {
                        Console.WriteLine(
                            $"    Field: {field.Name} : {field.Type}");
                    }
                }
                else
                {
                    Console.WriteLine("    (no fields)");
                }

                // Methods
                if (cls.Methods.Any())
                {
                    foreach (var method in cls.Methods)
                    {
                        Console.WriteLine(
                            $"    Method: {method.Name}({method.Parameters})");
                    }
                }
                else
                {
                    Console.WriteLine("    (no methods)");
                }
            }
        }
        else
        {
            Console.WriteLine("  (no classes)");
        }

        // -----------------------------
        // Top-level procedures
        // -----------------------------
        if (unit.Procedures.Any())
        {
            foreach (var proc in unit.Procedures)
            {
                Console.WriteLine(
                    $"  Procedure: {proc.Name}({proc.Parameters})");
            }
        }
        else
        {
            Console.WriteLine("  (no top-level procedures)");
        }
    }
}



    private void LogSemanticProcedure(SemanticProcedure proc)
{
    Console.WriteLine($"Procedure: {proc.Name}");

    foreach (var p in proc.Parameters)
    {
        var types = _enricher.GetTypes();
        var semanticType = types.TryGetValue(p.Value, out var type) ? type : new NamedType { QualifiedName = p.Value };
        Console.WriteLine($"  Param {p.Key}: {FormatType(semanticType)}");
    }

    foreach (var write in proc.Writes)
        Console.WriteLine($"  Effect: Write:{write}");
    foreach (var read in proc.Reads)
        Console.WriteLine($"  Effect: Read:{read}");
    foreach (var create in proc.Creates)
        Console.WriteLine($"  Effect: Create:{create}");
    foreach (var call in proc.Calls)
        Console.WriteLine($"  Effect: Call:{call}");
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
