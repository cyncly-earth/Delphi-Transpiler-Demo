using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;
using Transpiler.AST;
using Transpiler.Semantics;

namespace DelphiTranspiler.Semantics
{
    /// <summary>
    /// Builder for creating and enriching semantic AST from Delphi AST units.
    /// Orchestrates AST construction and semantic analysis in one place.
    /// </summary>
    public class SemanticIrBuilder
    {
        /// <summary>
        /// Builds the semantic IR by constructing AST units and running enrichment.
        /// </summary>
        /// <param name="irType">Type of IR to return: "ui", "entity", "backend", or "all" (default)</param>
        /// <returns>Tuple of (uiJson, entityJson, backendJson)</returns>
        public (string uiJson, string entityJson, string backendJson) BuildSemanticIr(string irType = "all")
        {
            try
            {
                Console.WriteLine("Starting semantic enrichment for feature...");

                // --------------------------------------------------
                // STEP 1: Build AST object graph (from sample files)
                // --------------------------------------------------
                var astUnits = BuildAstUnits();

                // --------------------------------------------------
                // STEP 2: Run semantic enrichment
                // --------------------------------------------------
                var enricher = new SemanticEnrichmentPrototype();
                var runner = new SemanticEnrichmentRunner(enricher);

                var (uiJson, entityJson, backendJson) = runner.ProcessFeature(astUnits);

                Console.WriteLine("Semantic enrichment completed successfully.");
                
                return (uiJson, entityJson, backendJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Constructs the AST units representing the Delphi code.
        /// </summary>
        private List<AstUnit> BuildAstUnits()
        {
            return new List<AstUnit>
            {
                // =====================================================
                // classPerson.pas
                // =====================================================
                new AstUnit
                {
                    Name = "classPerson",

                    Classes = new List<AstClass>
                    {
                        new AstClass
                        {
                            Name = "TPerson",

                            Fields = new List<AstField>
                            {
                                new AstField { Name = "cID",     Type = "String"  },
                                new AstField { Name = "cClient", Type = "Integer" },
                                new AstField { Name = "cFirst",  Type = "String"  },
                                new AstField { Name = "cLast",   Type = "String"  },
                                new AstField { Name = "cNotes",  Type = "String"  }
                            },

                            Methods = new List<AstProcedure>
                            {
                                new AstProcedure
                                {
                                    Name = "Create",
                                    Kind = "constructor",
                                    Parameters =
                                        "nPersonID : Integer; nClient : Integer; " +
                                        "nLast : String; nFirst : String; nNotes : String",
                                    ReturnType = "",
                                    HasBody = true,
                                    Body = @"cID := nPersonID;
cClient := nClient;
cLast := nLast;
cFirst := nFirst;
cNotes := nNotes;"
                                },

                                new AstProcedure
                                {
                                    Name = "ToString",
                                    Kind = "function",
                                    Parameters = "",
                                    ReturnType = "String",
                                    HasBody = true,
                                    Body = @"Result := cFirst + ' ' + cLast;"
                                }
                            },

                            Span = new SourceSpan
                            {
                                StartLine = 1,
                                StartColumn = 1,
                                EndLine = 60,
                                EndColumn = 1
                            }
                        }
                    }
                },

                // =====================================================
                // PersonController.pas (duplicate)
                // =====================================================
                new AstUnit
                {
                    Name = "PersonController",

                    Procedures = new List<AstProcedure>
                    {
                        new AstProcedure
                        {
                            Name = "AddPerson",
                            Kind = "procedure",
                            Parameters = "Person : TPerson",
                            ReturnType = "",
                            HasBody = true,
                            Body = @"
with Module.mtPerson do
begin
  if not Active then Open;
  Append;
  FieldByName('PersonID').Value := Person.PersonID;
  FieldByName('ClientID').Value := Person.Client;
  FieldByName('LastName').Value := Person.Last;
  FieldByName('FirstName').Value := Person.First;
  FieldByName('Notes').Value := Person.Notes;
  Post;
end;
",
                            Span = new SourceSpan
                            {
                                StartLine = 1,
                                StartColumn = 1,
                                EndLine = 40,
                                EndColumn = 1
                            }
                        }
                    }
                },

                // =====================================================
                // PersonView.pas
                // =====================================================
                new AstUnit
                {
                    Name = "PersonView",

                    Classes = new List<AstClass>
                    {
                        new AstClass
                        {
                            Name = "TfrmPerson",

                            Fields = new List<AstField>
                            {
                                new AstField { Name = "edtFirst", Type = "TEdit" },
                                new AstField { Name = "edtLast",  Type = "TEdit" },
                                new AstField { Name = "edtNotes", Type = "TEdit" }
                            },

                            Methods = new List<AstProcedure>
                            {
                                new AstProcedure
                                {
                                    Name = "btnAddPersonClick",
                                    Kind = "procedure",
                                    Parameters = "Sender : TObject",
                                    ReturnType = "",
                                    HasBody = true,
                                    Body = @"
Person := TPerson.Create(
  0,
  1,
  edtLast.Text,
  edtFirst.Text,
  edtNotes.Text
);
AddPerson(Person);
ShowMessage('Person added successfully');
",
                                    Span = new SourceSpan
                                    {
                                        StartLine = 1,
                                        StartColumn = 1,
                                        EndLine = 50,
                                        EndColumn = 1
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
