using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Transpiler.Semantics;

namespace Transpiler.AST
{
    public static class DelphiParser
    {
        // Main entry point called by AstProcessor
        public static List<AstUnit> ParseUnits(string inputDirectory)
        {
            var units = new List<AstUnit>();

            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine($"[Parser] Error: Input directory '{inputDirectory}' not found.");
                return units;
            }

            var files = Directory.GetFiles(inputDirectory, "*.pas");

            foreach (var filePath in files)
            {
                Console.WriteLine($"[Parser] Parsing file: {Path.GetFileName(filePath)}...");
                string content = File.ReadAllText(filePath);
                var unit = ParseSingleUnit(content);
                units.Add(unit);
            }

            return units;
        }

        private static AstUnit ParseSingleUnit(string content)
        {
            var astUnit = new AstUnit();

            // 1. Extract Unit Name
            var unitMatch = Regex.Match(content, @"unit\s+(\w+);", RegexOptions.IgnoreCase);
            astUnit.Name = unitMatch.Success ? unitMatch.Groups[1].Value : "UnknownUnit";

            // 2. Extract Classes (Interface Section)
            astUnit.Classes = ParseClasses(content);

            // 3. Extract Global Procedures (Implementation Section)
            // We differentiate methods inside classes from global procedures
            astUnit.Procedures = ParseProcedures(content, astUnit.Classes);

            // 4. Extract Global Fields/Variables (var section in interface)
            astUnit.Fields = ParseGlobalVariables(content);

            return astUnit;
        }

        private static List<AstClass> ParseClasses(string content)
        {
            var classes = new List<AstClass>();
            
            // Regex to find "TName = class ... end;"
            // This is a simplified regex for the demo
            string classPattern = @"type\s+(\w+)\s*=\s*class(.*?)end;";
            var matches = Regex.Matches(content, classPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var newClass = new AstClass
                {
                    Name = match.Groups[1].Value,
                    Span = GetSpan(content, match.Index, match.Length)
                };

                string classBody = match.Groups[2].Value;

                // Parse Fields (look for "name : type;")
                var fieldMatches = Regex.Matches(classBody, @"^\s*(\w+)\s*:\s*(\w+);", RegexOptions.Multiline);
                foreach (Match fm in fieldMatches)
                {
                    newClass.Fields.Add(new AstField
                    {
                        Name = fm.Groups[1].Value,
                        Type = fm.Groups[2].Value,
                        Span = new SourceSpan() // Simplified
                    });
                }

                // Parse Methods Declarations inside class (constructor, procedure, function)
                var methodMatches = Regex.Matches(classBody, @"(procedure|function|constructor|destructor)\s+(\w+)(\((.*?)\))?(:.*?)?;", RegexOptions.IgnoreCase);
                foreach (Match mm in methodMatches)
                {
                    newClass.Methods.Add(new AstProcedure
                    {
                        Kind = mm.Groups[1].Value,
                        Name = mm.Groups[2].Value,
                        Parameters = mm.Groups[4].Success ? mm.Groups[4].Value : "",
                        ReturnType = mm.Groups[5].Success ? mm.Groups[5].Value.Replace(":", "").Trim() : "",
                        HasBody = true // Assuming implementation exists later
                    });
                }

                classes.Add(newClass);
            }

            return classes;
        }

        private static List<AstField> ParseGlobalVariables(string content)
        {
            var fields = new List<AstField>();
            
            // Look for "var" block in Interface
            var varMatch = Regex.Match(content, @"interface.*?var(.*?)(procedure|implementation)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            if (varMatch.Success)
            {
                string varBlock = varMatch.Groups[1].Value;
                var lines = varBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach(var line in lines)
                {
                    var match = Regex.Match(line, @"(\w+)\s*:\s*(\w+);");
                    if (match.Success)
                    {
                        fields.Add(new AstField
                        {
                            Name = match.Groups[1].Value,
                            Type = match.Groups[2].Value
                        });
                    }
                }
            }
            return fields;
        }

        private static List<AstProcedure> ParseProcedures(string content, List<AstClass> definedClasses)
        {
            var procedures = new List<AstProcedure>();

            // Split into lines to scan for implementations
            var lines = content.Split(new[] { '\r', '\n' });
            
            bool insideBody = false;
            int beginCount = 0;
            
            AstProcedure currentProc = null;
            List<string> currentBodyLines = new List<string>();
            int startLineIndex = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string lineLower = line.ToLower();

                // Start of a procedure implementation
                // Matches: "procedure Name(Params);" or "procedure Class.Name(Params);"
                if (!insideBody && (lineLower.StartsWith("procedure ") || lineLower.StartsWith("function ") || lineLower.StartsWith("constructor ") || lineLower.StartsWith("destructor ")))
                {
                    // Regex to parse signature
                    var match = Regex.Match(line, @"(procedure|function|constructor|destructor)\s+([\w\.]+)(\((.*?)\))?(:.*?)?;", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string fullName = match.Groups[2].Value;
                        string className = "";
                        string methodName = fullName;

                        // Check if it belongs to a class (e.g. TPerson.Create)
                        if (fullName.Contains("."))
                        {
                            var parts = fullName.Split('.');
                            className = parts[0];
                            methodName = parts[1];
                        }

                        // Create the AST object
                        currentProc = new AstProcedure
                        {
                            Kind = match.Groups[1].Value,
                            Name = methodName,
                            Parameters = match.Groups[4].Success ? match.Groups[4].Value : "",
                            ReturnType = match.Groups[5].Success ? match.Groups[5].Value.Replace(":", "").Trim() : "",
                            HasBody = true,
                            Span = new SourceSpan { StartLine = i + 1 }
                        };

                        startLineIndex = i + 1;
                        currentBodyLines.Clear();
                        
                        // If it's a class method, we need to attach it to the class, not the global list
                        // But for parsing simplicity, we track it here first.
                        
                        insideBody = true;
                        beginCount = 0; 
                        
                        // If the "begin" is on the same line (rare in Delphi style but possible)
                        if (lineLower.Contains("begin")) beginCount++;
                        continue;
                    }
                }

                if (insideBody)
                {
                    currentBodyLines.Add(lines[i]); // Keep indentation

                    // Very simple block counting
                    // Note: This is a basic parser. A real one handles comments/strings properly.
                    if (lineLower == "begin" || lineLower.StartsWith("begin ") || lineLower.EndsWith(" begin")) 
                        beginCount++;
                    
                    if (lineLower == "case" || lineLower.StartsWith("case ")) // Case statements also end with 'end'
                        beginCount++;

                    if (lineLower == "end;" || lineLower == "end." || lineLower == "end")
                    {
                        beginCount--;
                        if (beginCount <= 0)
                        {
                            // Function finished
                            insideBody = false;
                            currentProc.Body = string.Join(Environment.NewLine, currentBodyLines);
                            currentProc.Span.EndLine = i + 1;

                            // LOGIC: Is this a Class Method or Global Procedure?
                            // We check if the name was "Class.Method" earlier
                            string fullProcLine = lines[startLineIndex - 1];
                            var nameMatch = Regex.Match(fullProcLine, @"\s+([\w\.]+)\(", RegexOptions.IgnoreCase);
                            string extractedName = nameMatch.Success ? nameMatch.Groups[1].Value : currentProc.Name;

                            if (extractedName.Contains("."))
                            {
                                // Attach body to the existing Class definition
                                string cName = extractedName.Split('.')[0];
                                string mName = extractedName.Split('.')[1];
                                
                                var foundClass = definedClasses.Find(c => c.Name == cName);
                                var foundMethod = foundClass?.Methods.Find(m => m.Name == mName);
                                
                                if (foundMethod != null)
                                {
                                    foundMethod.Body = currentProc.Body;
                                    foundMethod.Span = currentProc.Span;
                                }
                            }
                            else
                            {
                                // It's a global procedure
                                procedures.Add(currentProc);
                            }
                        }
                    }
                }
            }

            return procedures;
        }

        // Helper to calculate Span
        private static SourceSpan GetSpan(string content, int index, int length)
        {
            // Calculate line numbers based on index (simplified)
            string before = content.Substring(0, index);
            int startLine = before.Split('\n').Length;
            int endLine = (before + content.Substring(index, length)).Split('\n').Length;

            return new SourceSpan { StartLine = startLine, EndLine = endLine };
        }
    }
}