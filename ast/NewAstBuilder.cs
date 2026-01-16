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
            var unit = new AstUnit
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                InterfaceSection = new AstSection(),
                ImplementationSection = new AstSection()
            };

            Console.WriteLine($"Building AST for: {unit.Name}");

            try
            {
                // Extract uses clauses
                unit.InterfaceSection.Uses = ExtractUsesClause(parseTreeText, isInterface: true);
                unit.ImplementationSection.Uses = ExtractUsesClause(parseTreeText, isInterface: false);

                // Extract classes (typically in interface, but can be in implementation)
                unit.InterfaceSection.Classes = ExtractClasses(parseTreeText);

                // Extract module-level procedures (separated into interface and implementation)
                ExtractModuleProcedures(parseTreeText, unit);

                // Extract global variables
                unit.InterfaceSection.Variables = ExtractGlobalVariables(parseTreeText);

                // Extract global constants
                unit.InterfaceSection.Constants = ExtractGlobalConstants(parseTreeText);

                Console.WriteLine($"  ✓ Successfully parsed");
                Console.WriteLine($"    Interface Classes: {unit.InterfaceSection.Classes.Count}");
                Console.WriteLine($"    Implementation Procedures: {unit.ImplementationSection.Procedures.Count}");
                Console.WriteLine($"    Global Variables: {unit.InterfaceSection.Variables.Count}");

                return unit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");
                throw;
            }
        }

        private List<string> ExtractUsesClause(string text, bool isInterface)
        {
            var uses = new List<string>();

            // Pattern: usesClause (ident unitNameList (ident (...) unitName (qualifiedIdent (ident name) ...)))
            string usesPattern = isInterface 
                ? @"usesFileClause\s*\([^)]*identList\s*\([^)]*\)"
                : @"implementationStart.*?usesClause";

            var matches = Regex.Matches(text, usesPattern, RegexOptions.Singleline);

            if (matches.Count > 0)
            {
                string usesBlock = matches[0].Value;
                // Extract individual unit names: ident (\w+)
                var unitMatches = Regex.Matches(usesBlock, @"ident\s+(\w+)");
                foreach (Match match in unitMatches)
                {
                    uses.Add(match.Groups[1].Value);
                }
            }

            return uses;
        }

        private List<AstClass> ExtractClasses(string text)
        {
            var classes = new List<AstClass>();

            // Pattern: classTypeDecl class ... qualifiedIdent ( ident ClassName )
            string classPattern = @"classTypeDecl\s+class\s+[^)]*?qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var classMatches = Regex.Matches(text, classPattern);

            foreach (Match match in classMatches)
            {
                string className = match.Groups[1].Value;
                
                // Find the complete class block
                int startPos = match.Index;
                int openBraces = 0;
                int endPos = startPos;
                bool foundEnd = false;

                for (int i = startPos; i < text.Length && !foundEnd; i++)
                {
                    if (text[i] == '(') openBraces++;
                    else if (text[i] == ')') 
                    {
                        openBraces--;
                        if (openBraces == 0) 
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
                        ClassType = "class",
                        ParentTypes = ExtractParentTypes(classBlock),
                        Fields = ExtractClassFields(classBlock),
                        Methods = ExtractClassMethods(classBlock),
                        Properties = ExtractClassProperties(classBlock),
                        Span = new SourceSpan()
                    };

                    classes.Add(astClass);
                }
            }

            return classes;
        }

        private List<string> ExtractParentTypes(string classText)
        {
            var parents = new List<string>();

            // Pattern: parent qualifiedIdent (ident ParentName)
            string parentPattern = @"parent\s+qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var matches = Regex.Matches(classText, parentPattern);

            foreach (Match match in matches)
            {
                parents.Add(match.Groups[1].Value);
            }

            return parents;
        }

        private List<AstField> ExtractClassFields(string classText)
        {
            var fields = new List<AstField>();

            // Pattern: classField ( identList ( ident FieldName ) : typeDecl (...) (typeId (...) (qualifiedIdent (ident TypeName))))
            string fieldPattern = @"classField\s*\(\s*identList\s*\(\s*ident\s+(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var fieldMatches = Regex.Matches(classText, fieldPattern);

            var fieldDict = new Dictionary<string, AstField>();

            foreach (Match match in fieldMatches)
            {
                string fieldName = match.Groups[1].Value;
                string fieldType = match.Groups[2].Value;

                // Check visibility context
                string visibility = "public"; // default
                if (classText.Substring(0, match.Index).Contains("private"))
                    visibility = "private";
                else if (classText.Substring(0, match.Index).Contains("protected"))
                    visibility = "protected";

                fieldDict[fieldName] = new AstField
                {
                    Names = new List<string> { fieldName },
                    TypeName = fieldType,
                    Visibility = visibility,
                    Span = new SourceSpan()
                };
            }

            return new List<AstField>(fieldDict.Values);
        }

        private List<AstProperty> ExtractClassProperties(string classText)
        {
            var properties = new List<AstProperty>();

            // Pattern: classProperty ... property (ident PropertyName) : typeDecl ... propertyReadSpec ... propertyWriteSpec
            string propPattern = @"classProperty\s*[^)]*property\s*\(\s*ident\s+(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var propMatches = Regex.Matches(classText, propPattern);

            foreach (Match match in propMatches)
            {
                string propName = match.Groups[1].Value;
                string propType = match.Groups[2].Value;

                // Extract read/write specifiers
                int propStart = match.Index;
                string propBlock = classText.Substring(propStart);
                int propEnd = propBlock.IndexOf("classProperty");
                if (propEnd < 0) propEnd = propBlock.Length;
                propBlock = propBlock.Substring(0, propEnd);

                // Simple heuristic: look for patterns like "read cFieldName"
                var readMatch = Regex.Match(propBlock, @"read\s+(\w+)");
                var writeMatch = Regex.Match(propBlock, @"write\s+(\w+)");

                properties.Add(new AstProperty
                {
                    Name = propName,
                    TypeName = propType,
                    ReadSpecifier = readMatch.Success ? readMatch.Groups[1].Value : "",
                    WriteSpecifier = writeMatch.Success ? writeMatch.Groups[1].Value : "",
                    Visibility = "public",
                    Span = new SourceSpan()
                });
            }

            return properties;
        }

        private List<AstProcedure> ExtractClassMethods(string classText)
        {
            var methods = new List<AstProcedure>();

            // Pattern: classMethod (methodKey keyword) (ident MethodName)
            string methodPattern = @"classMethod\s*\([^)]*\(methodKey\s+(\w+)\s*\)[^)]*\(ident\s+(\w+)\s*\)";
            var methodMatches = Regex.Matches(classText, methodPattern);

            var seenMethods = new HashSet<string>();

            foreach (Match match in methodMatches)
            {
                string methodType = match.Groups[1].Value.ToLower();
                string methodName = match.Groups[2].Value;

                if (seenMethods.Contains(methodName))
                    continue;

                seenMethods.Add(methodName);

                var method = new AstProcedure
                {
                    Name = methodName,
                    ProcedureType = methodType,
                    Parameters = ExtractProcedureParameters(classText, methodName),
                    ReturnType = methodType == "function" ? "unknown" : "",
                    IsForwardDeclaration = false,
                    Visibility = "public",
                    Directives = new List<string>(),
                    LocalConstants = new List<AstConstDeclaration>(),
                    LocalVariables = new List<AstVarDeclaration>(),
                    LocalTypes = new List<AstTypeDeclaration>(),
                    Body = new List<AstStatement>(),
                    Span = new SourceSpan()
                };

                methods.Add(method);
            }

            return methods;
        }

        private void ExtractModuleProcedures(string text, AstUnit unit)
        {
            // Find procedure/function declarations
            string procPattern = @"(procDecl|function)\s*\([^)]*\(ident\s+(\w+)\s*\)";
            var procMatches = Regex.Matches(text, procPattern);

            var seenProcs = new HashSet<string>();
            var interfaceProcs = new HashSet<string>();

            // First pass: identify which procedures are in the interface (forward declarations)
            string interfaceSection = text;
            int implStart = text.IndexOf("implementationStart");
            if (implStart > 0)
            {
                interfaceSection = text.Substring(0, implStart);
            }

            var interfaceMatches = Regex.Matches(interfaceSection, procPattern);
            foreach (Match match in interfaceMatches)
            {
                interfaceProcs.Add(match.Groups[2].Value);
            }

            // Second pass: add all procedures to implementation, mark as forward if in interface
            foreach (Match match in procMatches)
            {
                string procName = match.Groups[2].Value;

                if (seenProcs.Contains(procName))
                    continue;

                seenProcs.Add(procName);

                string procKind = match.Groups[1].Value == "function" ? "function" : "procedure";

                var procedure = new AstProcedure
                {
                    Name = procName,
                    ProcedureType = procKind,
                    Parameters = ExtractProcedureParameters(text, procName),
                    ReturnType = procKind == "function" ? "unknown" : "",
                    IsForwardDeclaration = interfaceProcs.Contains(procName),
                    Visibility = "",
                    Directives = new List<string>(),
                    LocalConstants = new List<AstConstDeclaration>(),
                    LocalVariables = ExtractProcedureLocalVariables(text, procName),
                    LocalTypes = new List<AstTypeDeclaration>(),
                    Body = ExtractProcedureBody(text, procName),
                    Span = new SourceSpan()
                };

                if (interfaceProcs.Contains(procName))
                {
                    unit.InterfaceSection.Procedures.Add(procedure);
                }
                else
                {
                    unit.ImplementationSection.Procedures.Add(procedure);
                }
            }
        }

        private List<AstParameter> ExtractProcedureParameters(string text, string procName)
        {
            var parameters = new List<AstParameter>();

            // Pattern: parameterDeclList (parameterDecl ( ... identList (ident ParamName) : typeDecl (...) (typeId (...) (qualifiedIdent (ident TypeName)))))
            string paramPattern = @"parameterDeclList\s*\([^)]*\(identList\s*\([^)]*ident\s+(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var paramMatches = Regex.Matches(text, paramPattern);

            var seenParams = new HashSet<string>();

            foreach (Match match in paramMatches)
            {
                string paramName = match.Groups[1].Value;
                string paramType = match.Groups[2].Value;

                if (seenParams.Contains(paramName))
                    continue;

                seenParams.Add(paramName);

                // Detect parameter mode (var, const, out)
                string paramMode = "";
                int matchStart = Math.Max(0, match.Index - 50);
                string context = text.Substring(matchStart, Math.Min(50, match.Index - matchStart));
                if (context.Contains("var ")) paramMode = "var";
                else if (context.Contains("const ")) paramMode = "const";
                else if (context.Contains("out ")) paramMode = "out";

                parameters.Add(new AstParameter
                {
                    Names = new List<string> { paramName },
                    ParameterType = paramMode,
                    TypeName = paramType,
                    DefaultValue = ""
                });
            }

            return parameters;
        }

        private List<AstVarDeclaration> ExtractProcedureLocalVariables(string text, string procName)
        {
            var localVars = new List<AstVarDeclaration>();

            // Find the procedure block for this specific procedure
            string procPattern = $@"(procDecl|function)\s*\([^)]*\(ident\s+{procName}\s*\)";
            var procMatch = Regex.Match(text, procPattern);

            if (!procMatch.Success)
                return localVars;

            // Find variables declared after the procedure
            int startPos = procMatch.Index;
            int openParens = 0;
            int endPos = startPos;

            for (int i = startPos; i < text.Length; i++)
            {
                if (text[i] == '(') openParens++;
                else if (text[i] == ')') 
                {
                    openParens--;
                    if (openParens == 0)
                    {
                        endPos = i;
                        break;
                    }
                }
            }

            string procBlock = text.Substring(startPos, Math.Min(endPos - startPos + 1, text.Length - startPos));

            // Pattern: varSection (identList (ident VarName) : typeDecl (...) (typeId (...) (qualifiedIdent (ident TypeName))))
            string varPattern = @"varSection\s*\([^)]*\(identList\s*\([^)]*ident\s+(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var varMatches = Regex.Matches(procBlock, varPattern);

            var seenVars = new HashSet<string>();

            foreach (Match match in varMatches)
            {
                string varName = match.Groups[1].Value;
                string varType = match.Groups[2].Value;

                if (seenVars.Contains(varName))
                    continue;

                seenVars.Add(varName);

                localVars.Add(new AstVarDeclaration
                {
                    Names = new List<string> { varName },
                    TypeName = varType,
                    InitialValue = "",
                    Span = new SourceSpan()
                });
            }

            return localVars;
        }

        private List<AstStatement> ExtractProcedureBody(string text, string procName)
        {
            var statements = new List<AstStatement>();

            // Find the procedure block
            string procPattern = $@"(procDecl|function)\s*\([^)]*\(ident\s+{procName}\s*\)";
            var procMatch = Regex.Match(text, procPattern);

            if (!procMatch.Success)
                return statements;

            int startPos = procMatch.Index;
            int openParens = 0;
            int endPos = startPos;

            for (int i = startPos; i < text.Length; i++)
            {
                if (text[i] == '(') openParens++;
                else if (text[i] == ')') 
                {
                    openParens--;
                    if (openParens == 0)
                    {
                        endPos = i;
                        break;
                    }
                }
            }

            string procBlock = text.Substring(startPos, Math.Min(endPos - startPos + 1, text.Length - startPos));

            // Extract statements: assignments, procedure calls, if, while, for, try/finally, with
            ExtractStatementsFromBlock(procBlock, statements);

            return statements;
        }

        private void ExtractStatementsFromBlock(string blockText, List<AstStatement> statements)
        {
            // Assignments: ident := expression
            var assignPattern = @"(\w+)\s*:=\s*([^;]+)";
            var assignMatches = Regex.Matches(blockText, assignPattern);

            foreach (Match match in assignMatches)
            {
                statements.Add(new AstAssignment
                {
                    Target = match.Groups[1].Value,
                    Value = match.Groups[2].Value,
                    Span = new SourceSpan()
                });
            }

            // Procedure calls: ProcName(...) or ProcName;
            var callPattern = @"(\w+)\s*\(";
            var callMatches = Regex.Matches(blockText, callPattern);

            foreach (Match match in callMatches)
            {
                string procName = match.Groups[1].Value;
                
                // Skip if it's a keyword
                if (procName == "if" || procName == "while" || procName == "for")
                    continue;

                statements.Add(new AstProcedureCall
                {
                    ProcedureName = procName,
                    Arguments = new List<string>(),
                    Span = new SourceSpan()
                });
            }

            // If statements
            if (blockText.Contains("if "))
            {
                var ifPattern = @"if\s+([^t]*)\s+then";
                var ifMatches = Regex.Matches(blockText, ifPattern);

                foreach (Match match in ifMatches)
                {
                    statements.Add(new AstIfStatement
                    {
                        Condition = match.Groups[1].Value.Trim(),
                        ThenBranch = new List<AstStatement>(),
                        ElseBranch = new List<AstStatement>(),
                        Span = new SourceSpan()
                    });
                }
            }

            // Try-finally (simplified)
            if (blockText.Contains("try"))
            {
                statements.Add(new AstTryStatement
                {
                    TryBlock = new List<AstStatement>(),
                    ExceptHandlers = new List<AstExceptionHandler>(),
                    FinallyBlock = new List<AstStatement>(),
                    IsTryFinally = true,
                    Span = new SourceSpan()
                });
            }

            // With statement (simplified)
            if (blockText.Contains("with "))
            {
                var withPattern = @"with\s+([^d]*)\s+do";
                var withMatch = Regex.Match(blockText, withPattern);

                if (withMatch.Success)
                {
                    statements.Add(new AstWithStatement
                    {
                        WithItems = new List<string> { withMatch.Groups[1].Value.Trim() },
                        Body = new List<AstStatement>(),
                        Span = new SourceSpan()
                    });
                }
            }
        }

        private List<AstVarDeclaration> ExtractGlobalVariables(string text)
        {
            var globalVars = new List<AstVarDeclaration>();

            // Pattern: varSection (identList (ident VarName) : typeDecl (...) (typeId (...) (qualifiedIdent (ident TypeName))))
            string varPattern = @"varSection\s*\([^)]*\(identList\s*\([^)]*ident\s+(\w+)\s*\)[^:]*:\s*\(typeDecl[^)]*\(typeId[^)]*\(qualifiedIdent\s*\(\s*ident\s+(\w+)\s*\)";
            var varMatches = Regex.Matches(text, varPattern);

            var seenVars = new HashSet<string>();

            foreach (Match match in varMatches)
            {
                string varName = match.Groups[1].Value;
                string varType = match.Groups[2].Value;

                if (seenVars.Contains(varName))
                    continue;

                seenVars.Add(varName);

                globalVars.Add(new AstVarDeclaration
                {
                    Names = new List<string> { varName },
                    TypeName = varType,
                    InitialValue = "",
                    Span = new SourceSpan()
                });
            }

            return globalVars;
        }

        private List<AstConstDeclaration> ExtractGlobalConstants(string text)
        {
            var globalConsts = new List<AstConstDeclaration>();

            // Pattern: constSection (ident ConstName = value)
            string constPattern = @"constSection\s*\([^)]*\(ident\s+(\w+)\s*=\s*([^)]+)\)";
            var constMatches = Regex.Matches(text, constPattern);

            foreach (Match match in constMatches)
            {
                globalConsts.Add(new AstConstDeclaration
                {
                    Name = match.Groups[1].Value,
                    TypeName = "",
                    Value = match.Groups[2].Value,
                    Span = new SourceSpan()
                });
            }

            return globalConsts;
        }
    }
}
