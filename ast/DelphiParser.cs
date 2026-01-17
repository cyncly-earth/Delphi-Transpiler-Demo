using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Transpiler.Semantics; // <--- This import is critical

namespace Transpiler.AST
{
    public static class DelphiParser
    {
        public static List<AstUnit> ParseUnits(string inputDirectory)
        {
            var units = new List<AstUnit>();
            if (!Directory.Exists(inputDirectory)) return units;

            var files = Directory.GetFiles(inputDirectory, "*.*")
                                 .Where(s => s.EndsWith(".pas", StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in files)
            {
                string content = File.ReadAllText(filePath);
                units.Add(ParseSingleUnit(content));
            }
            return units;
        }

        private static AstUnit ParseSingleUnit(string content)
        {
            var unit = new AstUnit();

            var nameMatch = Regex.Match(content, @"unit\s+(\w+);", RegexOptions.IgnoreCase);
            unit.Name = nameMatch.Success ? nameMatch.Groups[1].Value : "Unknown";

            var interfaceMatch = Regex.Match(content, @"interface(.*?)implementation", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var implementationMatch = Regex.Match(content, @"implementation(.*?)end\.", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            string interfaceText = interfaceMatch.Success ? interfaceMatch.Groups[1].Value : "";
            string implementationText = implementationMatch.Success ? implementationMatch.Groups[1].Value : "";

            unit.Uses = ParseUses(interfaceText);
            unit.Uses.AddRange(ParseUses(implementationText));
            unit.Uses = unit.Uses.Distinct().ToList();

            unit.Classes = ParseClassesInInterface(interfaceText, content);
            unit.Fields = ParseGlobalVars(interfaceText, content);

            ParseImplementationBodies(implementationText, unit, content);

            return unit;
        }

        private static List<string> ParseUses(string text)
        {
            var list = new List<string>();
            var match = Regex.Match(text, @"uses\s+([\w\s,.]+);", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var parts = match.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts) list.Add(p.Trim());
            }
            return list;
        }

        private static List<AstClass> ParseClassesInInterface(string text, string fullContent)
        {
            var classes = new List<AstClass>();
            var matches = Regex.Matches(text, @"type\s+(\w+)\s*=\s*class(.*?)end;", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                var cls = new AstClass { Name = m.Groups[1].Value };
                string body = m.Groups[2].Value;

                var fieldMatches = Regex.Matches(body, @"^\s*(\w+)\s*:\s*(\w+);", RegexOptions.Multiline);
                foreach (Match fm in fieldMatches)
                    cls.Fields.Add(new AstField { Name = fm.Groups[1].Value, Type = fm.Groups[2].Value });

                var propMatches = Regex.Matches(body, @"property\s+(\w+)\s*:\s*(\w+)", RegexOptions.IgnoreCase);
                foreach (Match pm in propMatches)
                    cls.Fields.Add(new AstField { Name = pm.Groups[1].Value, Type = pm.Groups[2].Value });

                var methodMatches = Regex.Matches(body, @"(procedure|function|constructor|destructor)\s+(\w+)(\((.*?)\))?(:.*?)?;", RegexOptions.IgnoreCase);
                foreach (Match mm in methodMatches)
                {
                    cls.Methods.Add(new AstProcedure
                    {
                        Kind = mm.Groups[1].Value.ToLower(),
                        Name = mm.Groups[2].Value,
                        Parameters = mm.Groups[4].Success ? mm.Groups[4].Value : "",
                        ReturnType = mm.Groups[5].Success ? mm.Groups[5].Value.Replace(":", "").Replace(";", "").Trim() : ""
                    });
                }
                classes.Add(cls);
            }
            return classes;
        }

        private static List<AstField> ParseGlobalVars(string text, string fullContent)
        {
            var list = new List<AstField>();
            var match = Regex.Match(text, @"var\s+(.*?)(procedure|function|implementation|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var lines = match.Groups[1].Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var vm = Regex.Match(line, @"(\w+)\s*:\s*(\w+);");
                    if (vm.Success) list.Add(new AstField { Name = vm.Groups[1].Value, Type = vm.Groups[2].Value });
                }
            }
            return list;
        }

        private static void ParseImplementationBodies(string implText, AstUnit unit, string fullContent)
        {
            var lines = implText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            AstProcedure? currentProc = null;
            List<string> bodyLines = new List<string>();
            bool readingBody = false;
            string currentFullMethodName = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string lineLower = line.ToLower();

                if (!readingBody && Regex.IsMatch(line, @"^(procedure|function|constructor|destructor)\s+", RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(line, @"(procedure|function|constructor|destructor)\s+([\w\.]+)(\((.*?)\))?(:.*?)?;?", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        currentFullMethodName = match.Groups[2].Value;
                        currentProc = new AstProcedure
                        {
                            Kind = match.Groups[1].Value,
                            Name = currentFullMethodName,
                            Parameters = match.Groups[4].Value,
                            ReturnType = match.Groups[5].Value.Replace(":", "").Replace(";", "").Trim(),
                            HasBody = true
                        };
                        readingBody = true;
                        bodyLines.Clear();
                        continue;
                    }
                }

                if (readingBody && currentProc != null)
                {
                    bodyLines.Add(lines[i]);
                    if (lineLower == "end;" || lineLower == "end.")
                    {
                        int beginCount = bodyLines.Count(l => l.Trim().ToLower().StartsWith("begin"));
                        int endCount = bodyLines.Count(l => l.Trim().ToLower().StartsWith("end") || l.Trim().ToLower() == "end;");
                        
                        if (beginCount <= endCount || lineLower == "end.") 
                        {
                            currentProc.Body = string.Join(Environment.NewLine, bodyLines);
                            AssignMethodToOwner(unit, currentProc, currentFullMethodName);
                            readingBody = false;
                            currentProc = null;
                        }
                    }
                }
            }
        }

        private static void AssignMethodToOwner(AstUnit unit, AstProcedure proc, string fullName)
        {
            if (fullName.Contains("."))
            {
                var parts = fullName.Split('.');
                string className = parts[0];
                string methodName = parts[1];

                var cls = unit.Classes.FirstOrDefault(c => c.Name.Equals(className, StringComparison.OrdinalIgnoreCase));
                if (cls != null)
                {
                    var existing = cls.Methods.FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.Body = proc.Body;
                        existing.HasBody = true;
                    }
                    else
                    {
                        proc.Name = methodName;
                        cls.Methods.Add(proc);
                    }
                }
            }
            else
            {
                unit.Procedures.Add(proc);
            }
        }
    }
}