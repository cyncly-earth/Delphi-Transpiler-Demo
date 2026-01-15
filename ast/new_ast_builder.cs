using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Transpiler.AST;

namespace DelphiTranspiler.AST
{
    public class NewAstBuilder
    {
        public AstUnit BuildFromParseTree(string parseTreeText, string fileName)
        {
            var unit = new AstUnit();
            unit.Name = Path.GetFileNameWithoutExtension(fileName);

            Console.WriteLine($"Building AST for: {unit.Name}");

            try
            {
                // Extract classes
                unit.Classes = ExtractClasses(parseTreeText);

                // Extract procedures
                unit.Procedures = ExtractProcedures(parseTreeText, false);

                // Extract global fields
                unit.Fields = ExtractGlobalFields(parseTreeText);

                Console.WriteLine($"  ✓ Successfully parsed");
                Console.WriteLine($"    Classes: {unit.Classes.Count}");
                Console.WriteLine($"    Procedures: {unit.Procedures.Count}");
                Console.WriteLine($"    Fields: {unit.Fields.Count}");

                return unit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");
                throw;
            }
        }

        private List<AstClass> ExtractClasses(string text)
        {
            var classes = new List<AstClass>();

            // Find all class declarations
            string classPattern = @"classTypeDecl\s+class\s+[^)]*?qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)";
            var classMatches = Regex.Matches(text, classPattern);

            foreach (Match match in classMatches)
            {
                string className = match.Groups[1].Value;
                
                // Find the complete class block for this class
                int startPos = match.Index;
                int openParens = 0;
                int endPos = startPos;
                bool foundEnd = false;

                for (int i = startPos; i < text.Length && !foundEnd; i++)
                {
                    if (text[i] == '(') openParens++;
                    else if (text[i] == ')') 
                    {
                        openParens--;
                        if (openParens == 0) 
                        {
                            endPos = i;
                            foundEnd = true;
                        }
                    }
                }

                if (foundEnd)
                {
                    string classBlock = text.Substring(startPos, endPos - startPos + 1);

                    var astClass = new AstClass
                    {
                        Name = className,
                        Fields = ExtractClassFields(classBlock),
                        Methods = ExtractClassMethods(classBlock),
                        Span = new SourceSpan()
                    };

                    classes.Add(astClass);
                }
            }

            return classes;
        }

        private List<AstField> ExtractClassFields(string classText)
        {
            var fields = new List<AstField>();

            // Find all field declarations: classField (identList (...) : (typeDecl ...))
            string fieldPattern = @"classField\s*\(\s*identList\s*\(\s*ident\s*(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)";
            var fieldMatches = Regex.Matches(classText, fieldPattern);

            foreach (Match match in fieldMatches)
            {
                string fieldName = match.Groups[1].Value;
                string fieldType = match.Groups[2].Value;

                fields.Add(new AstField
                {
                    Name = fieldName,
                    Type = fieldType,
                    Span = new SourceSpan()
                });
            }

            return fields;
        }

        private List<AstProcedure> ExtractClassMethods(string classText)
        {
            var methods = new List<AstProcedure>();

            // Find all class methods: classMethod (methodKey ...) (ident ...)
            string methodPattern = @"classMethod\s*\(\s*(methodKey\s+(\w+)|function)\s+[^)]*\(ident\s*(\w+)\s*\)";
            var methodMatches = Regex.Matches(classText, methodPattern);

            foreach (Match match in methodMatches)
            {
                string methodType = match.Groups[2].Value ?? "function";
                string methodName = match.Groups[3].Value;

                var method = new AstProcedure
                {
                    Name = methodName,
                    Kind = methodType.ToLower(),
                    Parameters = "",
                    ReturnType = methodType == "function" ? "unknown" : "",
                    HasBody = false,
                    Body = "",
                    Span = new SourceSpan()
                };

                methods.Add(method);
            }

            return methods;
        }

        private List<AstProcedure> ExtractProcedures(string text, bool functionsOnly)
        {
            var procedures = new List<AstProcedure>();

            // Find procedure/function declarations at module level
            string pattern = @"(procDecl|function)\s*\([^)]*\(ident\s*(\w+)\s*\)";
            var matches = Regex.Matches(text, pattern);

            var seenNames = new HashSet<string>();

            foreach (Match match in matches)
            {
                string procName = match.Groups[2].Value;

                // Avoid duplicates
                if (seenNames.Contains(procName))
                    continue;

                seenNames.Add(procName);

                var procedure = new AstProcedure
                {
                    Name = procName,
                    Kind = match.Groups[1].Value == "function" ? "function" : "procedure",
                    Parameters = "",
                    ReturnType = "",
                    HasBody = false,
                    Body = "",
                    Span = new SourceSpan()
                };

                procedures.Add(procedure);
            }

            return procedures;
        }

        private List<AstField> ExtractGlobalFields(string text)
        {
            var fields = new List<AstField>();

            // Find global variable declarations
            string varPattern = @"varSection\s*\([^)]+identListFlat\s*\(\s*ident\s*(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)";
            var varMatches = Regex.Matches(text, varPattern, RegexOptions.Singleline);

            foreach (Match match in varMatches)
            {
                string varName = match.Groups[1].Value;
                string varType = match.Groups[2].Value;

                fields.Add(new AstField
                {
                    Name = varName,
                    Type = varType,
                    Span = new SourceSpan()
                });
            }

            return fields;
        }
    }
}
