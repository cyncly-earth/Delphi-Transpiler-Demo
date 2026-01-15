using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;
using Transpiler.AST;
using Transpiler.Semantics;

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
                // STEP 1: Build AST object graph (from sample files)
                // --------------------------------------------------
               var astUnits = new List<AstUnit>
{

    // =====================================================
// PersonController.pas
// =====================================================
new AstUnit
{
    Name = "PersonController",

    Procedures = new List<AstProcedure>
    {
        // -------------------------
        // AddPerson
        // -------------------------
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
        },

        // -------------------------
        // DeletePerson  ✅ NEW
        // -------------------------
        new AstProcedure
        {
            Name = "DeletePerson",
            Kind = "procedure",
            Parameters = "PersonID : Integer",
            ReturnType = "",
            HasBody = true,

            Body = @"
with Module.mtPerson do
begin
  if not Active then Open;
  if Locate('PersonID', PersonID, []) then
    Delete;
end;
",

            Span = new SourceSpan
            {
                StartLine = 42,
                StartColumn = 1,
                EndLine = 65,
                EndColumn = 1
            }
        }
    }
},
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
    // PersonController.pas
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


             

                

                // --------------------------------------------------
                // STEP 2: Run semantic enrichment
                // --------------------------------------------------
                var enricher = new SemanticEnrichmentPrototype();
                var runner = new SemanticEnrichmentRunner(enricher);

                runner.ProcessFeature(astUnits);

                Console.WriteLine("Semantic enrichment completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }

        private static List<string> ParseParameters(string parameters)
        {
            if (string.IsNullOrEmpty(parameters)) return new List<string>();
            // Simple split by ; and take param names
            return parameters.Split(';').Select(p => p.Trim().Split(':')[0].Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
        }

        private static List<string> ParseBody(string body)
        {
            if (string.IsNullOrEmpty(body)) return new List<string>();
            // Simple split by newlines and filter
            return body.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
        }
    }
}
