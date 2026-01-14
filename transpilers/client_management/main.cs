using System;
using System.Collections.Generic;
using DelphiTranspiler.Semantics;
using DelphiTranspiler.Semantics.AstNodes;
using Transpiler.AST;

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

            // ✅ CLASS FIELDS
            Fields = new List<AstField>
            {
                new AstField { Name = "cID", Type = "Integer" },
                new AstField { Name = "cClient", Type = "Integer" },
                new AstField { Name = "cFirst", Type = "String" },
                new AstField { Name = "cLast", Type = "String" },
                new AstField { Name = "cNotes", Type = "String" },
                new AstField { Name = "Contacts", Type = "TList" }
            },

            // ✅ METHODS (only what you decided to keep)
            Methods = new List<AstProcedure>
            {
                new AstProcedure
                {
                    Name = "Add",
                    Parameters = "Contact : TContact",
                    ReturnType = "",
                    HasBody = true,
                    Body = @"Contacts.Add(Contact);",
                    Span = new SourceSpan
                    {
                        StartLine = 74,
                        StartColumn = 1,
                        EndLine = 77,
                        EndColumn = 1
                    }
                }
            },

            Span = new SourceSpan
            {
                StartLine = 6,
                StartColumn = 1,
                EndLine = 88,
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
                                Parameters = "Person : TPerson; Contacts : Array of TContact",
                                ReturnType = "", // Delphi procedure → no return type
                                HasBody = true,

                                Body = @"
with Module do
begin
  Module.mtPerson.Open;
  Module.mtPerson.Append;
  PersonFields(Person);
  Module.mtPerson.Post;

  if Length(Contacts) > 0 then
  begin
    mtContact.Open;
    for I := 0 to Length(Contacts)-1 do
    begin
      mtContact.Append;
      ContactFields(Contacts[I], Person.PersonID);
    end;
    mtContact.Post;
  end;
end;
",

                                Span = new SourceSpan
                                {
                                    StartLine = 96,
                                    StartColumn = 1,
                                    EndLine = 124,
                                    EndColumn = 1
                                }
                            }
                        }
                    },

                    // =====================================================
                    // PersonView.pas (UI unit – included but later ignored)
                    // =====================================================
                    new AstUnit
                    {
                        Name = "PersonView",

                        Procedures = new List<AstProcedure>
                        {

                            new AstProcedure
                            {
                                Name = "AddPersonToListBox",
                                Parameters = "Person : TPerson",
                                ReturnType = "",
                                HasBody = true,
                                Body = @"
Item := TListBoxItem.Create(MainForm.lbPeople);
Item.Text := Person.PersonID.ToString;
Item.ItemData.Detail := Person.ToString;
Item.Data := Person;
Item.StyleLookup := 'HeaderListBoxItem';
MainForm.lbPeople.AddObject(Item);
",
                                Span = new SourceSpan { StartLine = 39, StartColumn = 1, EndLine = 55, EndColumn = 1 }
                            },

                            new AstProcedure
                            {
                                Name = "AddContactToListBox",
                                Parameters = "Contact : TContact",
                                ReturnType = "",
                                HasBody = true,
                                Body = @"
Item := TListBoxItem.Create(MainForm.lbPeople);
Item.Text := 'Contact' + Contact.PersonID.ToString;
Item.Data := Contact;
Item.ItemData.Detail := Contact.ToString;
Item.StyleLookup := 'SubListBoxItem';
MainForm.lbPeople.AddObject(Item);
",
                                Span = new SourceSpan { StartLine = 58, StartColumn = 1, EndLine = 73, EndColumn = 1 }
                            },

                            new AstProcedure
                            {
                                Name = "AddPersonSave",
                                Parameters = "",
                                ReturnType = "",
                                HasBody = true,
                                Body = @"
Person := TPerson.Create(...);
PersonController.AddPerson(Person, Contacts);
AddPersonToListBox(Person);
ClientsTabView.ClientSave;
",
                                Span = new SourceSpan { StartLine = 275, StartColumn = 1, EndLine = 334, EndColumn = 1 }
                            }
                        }
                    }
                };

                // Convert AstUnits to AstNodes
                var asts = new List<AstNode>();
                foreach (var unit in astUnits)
                {
                    foreach (var cls in unit.Classes ?? new List<AstClass>())
                    {
                        var classNode = new ClassDeclNode
                        {
                            NodeType = "ClassDecl",
                            Name = cls.Name,
                            Fields = new List<string>() // Assuming no fields in sample
                        };
                        asts.Add(classNode);

                        foreach (var method in cls.Methods ?? new List<AstProcedure>())
                        {
                            var procNode = new ProcedureDeclNode
                            {
                                NodeType = "ProcedureDecl",
                                Name = method.Name,
                                Params = ParseParameters(method.Parameters),
                                Body = ParseBody(method.Body)
                            };
                            asts.Add(procNode);
                        }
                    }

                    foreach (var proc in unit.Procedures ?? new List<AstProcedure>())
                    {
                        var procNode = new ProcedureDeclNode
                        {
                            NodeType = "ProcedureDecl",
                            Name = proc.Name,
                            Params = ParseParameters(proc.Parameters),
                            Body = ParseBody(proc.Body)
                        };
                        asts.Add(procNode);
                    }
                }

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
