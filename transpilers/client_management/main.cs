using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;

namespace ClientManagementTranspiler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting semantic enrichment for feature...");

                // --------------------------------------------------
                // STEP 1: Build AST object graph (mock parser output)
                // --------------------------------------------------
                var asts = new List<AstNode>
                {
                    // classPerson.pas
                    new ClassDeclNode
                    {
                        NodeType = "ClassDecl",
                        Name = "TPerson",
                        Fields = { "cID", "cClient", "cFirst", "cLast", "cNotes" }
                    },

                    // PersonController.pas
                    new ProcedureDeclNode
                    {
                        NodeType = "ProcedureDecl",
                        Name = "AddPerson",
                        Params = { "Person", "Contacts" },
                        Body = { "with", "if", "for", "call" }
                    }

                    // NOTE:
                    // PersonView AST would ALSO be added here,
                    // but it is ignored by semantic enrichment → IR
                };

                // --------------------------------------------------
                // STEP 2: Run semantic enrichment
                // --------------------------------------------------
                var enricher = new SemanticEnrichmentPrototype();
                var runner = new SemanticEnrichmentRunner(enricher);

                runner.ProcessFeature(asts);

                Console.WriteLine("Semantic enrichment completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }
    }
}
