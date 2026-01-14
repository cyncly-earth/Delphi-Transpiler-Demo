using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;
using DelphiTranspiler.Semantics.SemanticModels;
using DelphiTranspiler.Semantics.IR;
using System.Text.Json;


public class SemanticEnrichmentRunner
{
    private readonly SemanticEnrichmentPrototype _enricher;

    public SemanticEnrichmentRunner(SemanticEnrichmentPrototype enricher)
    {
        _enricher = enricher;
    }

    public void ProcessFeature(IEnumerable<AstNode> astUnits)
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
        _enricher.ResolveProcedureTypes();
        _enricher.InferEffects();

        // =====================================================
        // STAGE 3: LOG SEMANTIC AST
        // =====================================================
        Console.WriteLine();
        Console.WriteLine("===== SEMANTIC AST =====");

        foreach (var proc in _enricher.GetSemanticProcedures())
        {
            LogSemanticProcedure(proc);
        }

        // =====================================================
        // STAGE 4: IR GENERATION + LOGGING
        // =====================================================
        var irGen = new IrGenerator();

        Console.WriteLine();
        Console.WriteLine("===== BACKEND IR =====");

        foreach (var proc in _enricher.GetSemanticProcedures())
        {
            var ir = irGen.Generate(proc);
            var json = JsonSerializer.Serialize(
        ir,
        new JsonSerializerOptions
        {
            WriteIndented = false // IMPORTANT: raw, not pretty
        });

    Console.WriteLine(json);
            
        }
    }

    // =====================================================
    // LOGGING HELPERS
    // =====================================================

    private void LogAstInput(IEnumerable<AstNode> asts)
{
    Console.WriteLine("===== AST INPUT =====");

    foreach (var ast in asts)
    {
        switch (ast)
        {
            case ClassDeclNode c:
                Console.WriteLine($"ClassDecl: {c.Name}");

                if (c.Fields.Any())
                {
                    foreach (var f in c.Fields)
                        Console.WriteLine($"  Field: {f}");
                }
                else
                {
                    Console.WriteLine("  (no fields)");
                }

                if (c.Methods.Any())
                {
                    foreach (var m in c.Methods)
                        Console.WriteLine($"  Method: {m}");
                }
                else
                {
                    Console.WriteLine("  (no methods)");
                }
                break;

            case ProcedureDeclNode p:
                Console.WriteLine($"ProcedureDecl: {p.Name}");
                Console.WriteLine($"  Params: {string.Join(", ", p.Params)}");
                break;
        }
    }
}


    private void LogSemanticProcedure(SemanticProcedure proc)
{
    Console.WriteLine($"Procedure: {proc.Name}");

    foreach (var p in proc.Parameters)
    {
        Console.WriteLine($"  Param {p.Name}: {FormatType(p.Type)}");
    }

    foreach (var e in proc.Effects)
    {
        Console.WriteLine($"  Effect: {e.Kind}:{e.Target}");
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
