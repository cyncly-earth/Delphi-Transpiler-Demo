using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;

public class SemanticEnrichmentRunner
{
    private readonly SemanticEnrichmentPrototype _enricher;

    public SemanticEnrichmentRunner(SemanticEnrichmentPrototype enricher)
    {
        _enricher = enricher;
    }

    public void ProcessFeature(IEnumerable<AstNode> astUnits)
    {
        // -----------------------------
        // PHASE 1: Load all AST nodes
        // -----------------------------
        foreach (var ast in astUnits)
        {
            _enricher.LoadUnit(ast);
        }

        // -----------------------------
        // PHASE 2: Semantic enrichment
        // -----------------------------
        _enricher.CollectTypes();
        _enricher.CollectProcedures();
        _enricher.ResolveProcedureTypes();
        _enricher.InferEffects();

        // -----------------------------
        // DEBUG OUTPUT (temporary)
        // -----------------------------
        Console.WriteLine("Semantic AST generated.");

        foreach (var proc in _enricher.GetSemanticProcedures())
        {
            Console.WriteLine($"Procedure: {proc.Symbol}");

            foreach (var p in proc.Parameters)
                Console.WriteLine($"  Param {p.Key}: {p.Value}");

            foreach (var e in proc.Effects)
                Console.WriteLine($"  Effect: {e}");
        }
    }
}
