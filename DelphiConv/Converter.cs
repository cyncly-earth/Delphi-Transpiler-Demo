using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;

namespace DelphiToCsConverter
{
    public class Converter
    {
        public SemanticSolution Process(DelphiUnit itemAst, DelphiUnit viewAst, DelphiUnit ctrlAst)
{
    var ir = new SemanticSolution();

    // =========================================================
    // 1. EXTRACT ENTITIES (From CalendarItem)
    // =========================================================
    if (itemAst?.InterfaceSection?.Classes != null)
    {
        foreach (var cls in itemAst.InterfaceSection.Classes)
        {
            var entity = new IrEntity { Name = cls.Name.Replace("T", "") }; // Remove 'T'

            var props = cls.Properties.Count > 0
                ? cls.Properties
                : cls.Fields.Select(f => new DelphiProperty
                {
                    Name = f.Names[0],
                    TypeName = f.TypeName
                }).ToList();

            foreach (var p in props)
            {
                string cleanName =
                    p.Name.StartsWith("c") && char.IsUpper(p.Name[1])
                        ? p.Name.Substring(1)
                        : p.Name;

                entity.Properties.Add(new IrProp
                {
                    Name = cleanName,
                    Type = MapType(p.TypeName),
                    IsKey = cleanName.EndsWith("ID") && entity.Properties.Count == 0
                });
            }

            // 🔍 ENTITY LOG
            Console.WriteLine("---- GENERATED SEMANTIC ENTITY ----");
            Console.WriteLine($"Entity Name: {entity.Name}");
            Console.WriteLine("Properties:");
            foreach (var prop in entity.Properties)
            {
                Console.WriteLine($"  - {prop.Name} | Type: {prop.Type} | IsKey: {prop.IsKey}");
            }
            Console.WriteLine("----------------------------------");

            ir.Entities.Add(entity);
        }
    }

    // =========================================================
    // 2. EXTRACT UI LOGIC (From CalendarView)
    // =========================================================
    if (viewAst?.ImplementationSection?.Procedures != null)
    {
        Console.WriteLine("---- EXTRACTING UI LOGIC ----");

        foreach (var proc in viewAst.ImplementationSection.Procedures)
        {
            Console.WriteLine($"Found View Procedure: {proc.Name}");

            if (proc.Name == "SetItemResource" || proc.Name == "AddBedroomSlot")
            {
                var logic = new IrLogic { Name = proc.Name };
                logic.Lines = TranspileBody(proc.Body);
                ir.LogicBlocks.Add(logic);

                // 🔍 LOGIC BLOCK LOG
                Console.WriteLine($"  ✓ Added Logic Block: {logic.Name}");
                Console.WriteLine($"    Lines Generated: {logic.Lines.Count}");

                foreach (var line in logic.Lines)
                {
                    Console.WriteLine($"      {line}");
                }
            }
            else
            {
                Console.WriteLine($"  - Skipped (not targeted)");
            }
        }

        Console.WriteLine("----------------------------------");
    }

    // =========================================================
    // 3. EXTRACT SERVICES (From CalendarController)
    // =========================================================
    if (ctrlAst?.InterfaceSection?.Procedures != null)
    {
        var svc = new IrService { Name = "CalendarService" };

        Console.WriteLine("---- EXTRACTING SERVICES ----");
        Console.WriteLine($"Service Name: {svc.Name}");

        foreach (var proc in ctrlAst.InterfaceSection.Procedures)
        {
            var paramList = proc.Parameters
                .Select(p => $"{MapType(p.TypeName)} {p.Names[0]}")
                .ToList();

            var method = new IrMethod
            {
                Name = proc.Name,
                Signature = string.Join(", ", paramList)
            };

            svc.Methods.Add(method);

            // 🔍 SERVICE METHOD LOG
            Console.WriteLine($"  - Method: {method.Name}");
            Console.WriteLine($"    Signature: {method.Signature}");
        }

        ir.Services.Add(svc);

        Console.WriteLine("----------------------------------");
    }

    return ir;
}


        // --- CORE RECURSIVE TRANSPILER ---
        // Converts JSON AST Nodes ($type: "ifStatement", etc) into C# Code Strings
        private List<string> TranspileBody(List<JsonElement> nodes, int indent = 0)
        {
            var code = new List<string>();
            string pad = new string(' ', indent * 4);

            foreach (var node in nodes)
            {
                if (!node.TryGetProperty("$type", out var typeProp)) continue;
                string type = typeProp.GetString();

                if (type == "assignment")
                {
                    string target = node.GetProperty("target").GetString();
                    string val = CleanExpression(node.GetProperty("value").GetString());
                    code.Add($"{pad}{target} = {val};");
                }
                else if (type == "ifStatement")
                {
                    string condition = CleanExpression(node.GetProperty("condition").GetString());
                    code.Add($"{pad}if ({condition})");
                    code.Add($"{pad}{{");
                    
                    if (node.TryGetProperty("thenBranch", out var thenB))
                        code.AddRange(TranspileBody(thenB.EnumerateArray().ToList(), indent + 1));
                    
                    code.Add($"{pad}}}");

                    if (node.TryGetProperty("elseBranch", out var elseB) && elseB.GetArrayLength() > 0)
                    {
                        code.Add($"{pad}else");
                        code.Add($"{pad}{{");
                        code.AddRange(TranspileBody(elseB.EnumerateArray().ToList(), indent + 1));
                        code.Add($"{pad}}}");
                    }
                }
                else if (type == "compoundStatement" || type == "withStatement")
                {
                    // Flatten these blocks
                    if (node.TryGetProperty("statements", out var stmts))
                        code.AddRange(TranspileBody(stmts.EnumerateArray().ToList(), indent));
                    if (node.TryGetProperty("body", out var body))
                        code.AddRange(TranspileBody(body.EnumerateArray().ToList(), indent));
                }
            }
            return code;
        }

        private string MapType(string t)
        {
            t = t.ToLower();
            if (t.Contains("integer")) return "int";
            if (t.Contains("double")) return "decimal";
            if (t.Contains("datetime") || t.Contains("date")) return "DateTime";
            return "string";
        }

        private string CleanExpression(string s)
        {
            // Fix Delphi syntax to C# syntax
            if (string.IsNullOrEmpty(s)) return "";
            
            // AccomID in [2..3] -> (AccomID >= 2 && AccomID <= 3)
            s = Regex.Replace(s, @"(\w+)\s*in\s*\[(\d+)\.\.(\d+)\]", 
                m => $"({m.Groups[1].Value} >= {m.Groups[2].Value} && {m.Groups[1].Value} <= {m.Groups[3].Value})");
            
            s = s.Replace("=", "=="); // Delphi equality
            s = s.Replace(":=", "="); // Delphi assignment fix
            
            return s;
        }
    }
}