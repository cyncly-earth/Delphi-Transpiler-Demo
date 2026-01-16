using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Transpiler.Semantics;

namespace Transpiler.AST
{
    public static class DelphiParser
    {
        public static List<AstUnit> ParseUnits(string inputDirectory)
        {
            var units = new List<AstUnit>();
            if (!Directory.Exists(inputDirectory)) return units;

            foreach (var filePath in Directory.GetFiles(inputDirectory, "*.pas"))
            {
                string content = File.ReadAllText(filePath);
                units.Add(ParseSingleUnit(content));
            }
            return units;
        }

        private static AstUnit ParseSingleUnit(string content)
        {
            var unit = new AstUnit();

            // 1. Get Unit Name
            var nameMatch = Regex.Match(content, @"unit\s+(\w+);", RegexOptions.IgnoreCase);
            unit.Name = nameMatch.Success ? nameMatch.Groups[1].Value : "Unknown";

            // 2. Split Interface (Definitions) and Implementation (Code)
            var interfaceMatch = Regex.Match(content, @"interface(.*?)implementation", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var implementationMatch = Regex.Match(content, @"implementation(.*?)end\.", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            string interfaceText = interfaceMatch.Success ? interfaceMatch.Groups[1].Value : "";
            string implementationText = implementationMatch.Success ? implementationMatch.Groups[1].Value : "";

            // 3. Parse Dependencies (Uses)
            unit.Uses = ParseUses(interfaceText);
            unit.Uses.AddRange(ParseUses(implementationText));
            unit.Uses = unit.Uses.Distinct().ToList();

            // 4. Parse Interface (Classes and Global Vars)
            unit.Classes = ParseClassesInInterface(interfaceText, content);
            unit.Fields = ParseGlobalVars(interfaceText, content);

            // 5. Parse Implementation (Bodies)
            ParseImplementationBodies(implementationText, unit, content);

            return unit;
        }

        // --- Helper: Parse Uses ---
        private static List<string> ParseUses(string text)
        {
            var usesList = new List<string>();
            var match = Regex.Match(text, @"uses\s+([\w\s,.]+);", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var raw = match.Groups[1].Value;
                var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts) usesList.Add(p.Trim());
            }
            return usesList;
        }

        // --- Helper: Parse Classes ---
        private static List<AstClass> ParseClassesInInterface(string text, string fullContent)
        {
            var classes = new List<AstClass>();
            // Regex: type TName = class ... end;
            var matches = Regex.Matches(text, @"type\s+(\w+)\s*=\s*class(.*?)end;", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                var cls = new AstClass
                {
                    Name = m.Groups[1].Value,
                    Span = GetSpan(fullContent, m.Value)
                };

                string body = m.Groups[2].Value;

                // Parse Fields (private/public variables)
                // Looks for: name : type;
                var fieldMatches = Regex.Matches(body, @"^\s*(\w+)\s*:\s*(\w+);", RegexOptions.Multiline);
                foreach (Match fm in fieldMatches)
                {
                    cls.Fields.Add(new AstField { Name = fm.Groups[1].Value, Type = fm.Groups[2].Value });
                }

                // Parse Properties (simplified)
                var propMatches = Regex.Matches(body, @"property\s+(\w+)\s*:\s*(\w+)", RegexOptions.IgnoreCase);
                foreach (Match pm in propMatches)
                {
                    // Treat properties as fields for AST simplicity in this step
                    cls.Fields.Add(new AstField { Name = pm.Groups[1].Value, Type = pm.Groups[2].Value });
                }

                // Parse Method Definitions
                var methodMatches = Regex.Matches(body, @"(procedure|function|constructor|destructor)\s+(\w+)(\((.*?)\))?(:.*?)?;", RegexOptions.IgnoreCase);
                foreach (Match mm in methodMatches)
                {
                    cls.Methods.Add(new AstProcedure
                    {
                        Kind = mm.Groups[1].Value.ToLower(),
                        Name = mm.Groups[2].Value,
                        Parameters = mm.Groups[4].Success ? mm.Groups[4].Value : "",
                        ReturnType = mm.Groups[5].Success ? mm.Groups[5].Value.Replace(":", "").Replace(";", "").Trim() : "",
                        HasBody = false // Will be filled later
                    });
                }
                classes.Add(cls);
            }
            return classes;
        }

        // --- Helper: Parse Global Variables ---
        private static List<AstField> ParseGlobalVars(string text, string fullContent)
        {
            var list = new List<AstField>();
            var match = Regex.Match(text, @"var\s+(.*?)(procedure|function|implementation|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var vars = match.Groups[1].Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var v in vars)
                {
                    var vm = Regex.Match(v, @"(\w+)\s*:\s*(\w+);");
                    if (vm.Success) list.Add(new AstField { Name = vm.Groups[1].Value, Type = vm.Groups[2].Value });
                }
            }
            return list;
        }

        // --- Helper: Parse Implementation Bodies (The Complex Part) ---
        private static void ParseImplementationBodies(string implText, AstUnit unit, string fullContent)
        {
            // Split into lines to scan statefully
            var lines = implText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            
            AstProcedure currentProc = null;
            List<string> bodyLines = new List<string>();
            bool readingBody = false;
            string currentFullMethodName = "";
            int startLine = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string lineLower = line.ToLower();

                // 1. Detect Start of Procedure
                if (!readingBody && Regex.IsMatch(line, @"^(procedure|function|constructor|destructor)\s+", RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(line, @"(procedure|function|constructor|destructor)\s+([\w\.]+)(\((.*?)\))?(:.*?)?;?", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        currentFullMethodName = match.Groups[2].Value;
                        string kind = match.Groups[1].Value;
                        string parameters = match.Groups[4].Value;
                        string returnType = match.Groups[5].Value.Replace(":", "").Replace(";", "").Trim();

                        currentProc = new AstProcedure
                        {
                            Kind = kind,
                            Name = currentFullMethodName, // Placeholder, might change if class method
                            Parameters = parameters,
                            ReturnType = returnType,
                            HasBody = true,
                            Span = new SourceSpan { StartLine = GetLineNumber(fullContent, line) }
                        };
                        
                        readingBody = true;
                        bodyLines.Clear();
                        startLine = currentProc.Span.StartLine;
                        continue; // Don't add signature to body
                    }
                }

                // 2. Read Body
                if (readingBody)
                {
                    bodyLines.Add(lines[i]); // Keep indentation

                    // 3. Detect End of Procedure
                    // A simple heuristic: "end;" at the start of a line, or "end."
                    // In a real parser we count begin/end, but for this structure "end;" usually closes the method.
                    if (lineLower == "end;" || lineLower == "end.")
                    {
                        // Check nesting (primitive check)
                        int beginCount = bodyLines.Count(l => l.Trim().ToLower().StartsWith("begin"));
                        int endCount = bodyLines.Count(l => l.Trim().ToLower().StartsWith("end") || l.Trim().ToLower() == "end;");
                        
                        // If balanced or simple end found
                        if (beginCount <= endCount || lineLower == "end.") 
                        {
                            currentProc.Body = string.Join(Environment.NewLine, bodyLines);
                            currentProc.Span.EndLine = startLine + bodyLines.Count;
                            
                            AssignMethodToOwner(unit, currentProc, currentFullMethodName);
                            
                            readingBody = false;
                            currentProc = null;
                        }
                    }
                }
            }
        }

        // --- Helper: Link Method Body to Class or Unit ---
        private static void AssignMethodToOwner(AstUnit unit, AstProcedure proc, string fullName)
        {
            if (fullName.Contains("."))
            {
                // It's a Class Method (e.g. TPerson.Create)
                var parts = fullName.Split('.');
                string className = parts[0];
                string methodName = parts[1];

                var cls = unit.Classes.FirstOrDefault(c => c.Name.Equals(className, StringComparison.OrdinalIgnoreCase));
                if (cls != null)
                {
                    // Find the definition in the interface and update it
                    var existingMethod = cls.Methods.FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
                    if (existingMethod != null)
                    {
                        existingMethod.Body = proc.Body;
                        existingMethod.Span = proc.Span;
                        existingMethod.HasBody = true;
                        // Use the precise parameters from implementation if interface was vague, or vice versa
                        if (string.IsNullOrEmpty(existingMethod.Parameters)) existingMethod.Parameters = proc.Parameters;
                    }
                    else
                    {
                        // Method wasn't in interface (private?), add it now
                        proc.Name = methodName;
                        cls.Methods.Add(proc);
                    }
                }
            }
            else
            {
                // It's a Global Procedure (e.g. AddPerson)
                unit.Procedures.Add(proc);
            }
        }

        // --- Helper: Calculate Line Number ---
        private static int GetLineNumber(string content, string lineContent)
        {
            int index = content.IndexOf(lineContent);
            if (index == -1) return 0;
            return content.Take(index).Count(c => c == '\n') + 1;
        }

        private static SourceSpan GetSpan(string content, string snippet)
        {
            int start = GetLineNumber(content, snippet);
            int lines = snippet.Count(c => c == '\n');
            return new SourceSpan { StartLine = start, EndLine = start + lines };
        }
    }
}