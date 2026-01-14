// /workspaces/Delphi-Transpiler-Demo/ast/ast_builder.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DelphiTranspiler.AST
{
    public class AstBuilder
    {
        public CompilationUnitNode BuildFromParseTree(string parseTreeText, string fileName)
        {
            var unit = new CompilationUnitNode();
            unit.Name = Path.GetFileNameWithoutExtension(fileName);
            
            Console.WriteLine($"Building AST for: {unit.Name}");
            
            try
            {
                // Parse the structured text format
                unit.InterfaceUses = ParseUsesClauses(parseTreeText, "interface");
                unit.ImplementationUses = ParseUsesClauses(parseTreeText, "implementation");
                
                unit.TypeDeclarations = ParseTypeDeclarations(parseTreeText);
                
                ParseGlobalDeclarations(parseTreeText, unit);
                
                ParseMethodsAndProcedures(parseTreeText, unit);
                
                Console.WriteLine($"  ✓ Successfully parsed");
                Console.WriteLine($"    Types: {unit.TypeDeclarations.Count}");
                Console.WriteLine($"    Procedures: {unit.Procedures.Count}");
                Console.WriteLine($"    Functions: {unit.Functions.Count}");
                
                return unit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");
                throw;
            }
        }
        
        private List<UsesClauseNode> ParseUsesClauses(string text, string section)
        {
            var usesClauses = new List<UsesClauseNode>();
            
            string pattern = $@"{section}\s*uses\s*\([^)]+\)";
            var match = Regex.Match(text, pattern, RegexOptions.Singleline);
            
            if (match.Success)
            {
                var usesNode = new UsesClauseNode();
                var unitMatches = Regex.Matches(match.Value, @"ident\s*(\w+(?:\.\w+)*)");
                foreach (Match unitMatch in unitMatches)
                {
                    usesNode.Units.Add(unitMatch.Groups[1].Value.Replace(".", "."));
                }
                usesClauses.Add(usesNode);
            }
            
            return usesClauses;
        }
        
        private List<TypeDeclarationNode> ParseTypeDeclarations(string text)
        {
            var types = new List<TypeDeclarationNode>();
            
            string pattern = @"typeDeclaration\s*\([^)]+\)";
            var matches = Regex.Matches(text, pattern, RegexOptions.Singleline);
            
            foreach (Match match in matches)
            {
                var nameMatch = Regex.Match(match.Value, @"qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)");
                if (nameMatch.Success)
                {
                    var typeNode = new TypeDeclarationNode
                    {
                        Name = nameMatch.Groups[1].Value
                    };
                    
                    if (match.Value.Contains("classDecl"))
                    {
                        typeNode.Type = ParseClassType(match.Value);
                    }
                    else
                    {
                        typeNode.Type = new SimpleTypeNode
                        {
                            Name = nameMatch.Groups[1].Value,
                            IsBuiltIn = IsBuiltInType(nameMatch.Groups[1].Value)
                        };
                    }
                    
                    types.Add(typeNode);
                }
            }
            
            return types;
        }
        
        private TypeNode ParseClassType(string classText)
        {
            var classNode = new ClassDeclarationNode();
            
            var nameMatch = Regex.Match(classText, @"classTypeDecl\s+class\s+[^)]*?qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)");
            if (nameMatch.Success)
            {
                classNode.Name = nameMatch.Groups[1].Value;
            }
            
            classNode.Fields = ParseClassFields(classText);
            classNode.Properties = ParseClassProperties(classText);
            classNode.Methods = ParseClassMethods(classText);
            
            return new ClassTypeNode
            {
                Name = classNode.Name,
                ClassDefinition = classNode
            };
        }
        
        private List<FieldDeclarationNode> ParseClassFields(string classText)
        {
            var fields = new List<FieldDeclarationNode>();
            
            string fieldPattern = @"classField\s*\([^)]+\)";
            var matches = Regex.Matches(classText, fieldPattern);
            
            foreach (Match match in matches)
            {
                var field = new FieldDeclarationNode();
                
                var nameMatches = Regex.Matches(match.Value, @"ident\s*(\w+)");
                foreach (Match nameMatch in nameMatches)
                {
                    field.Names.Add(nameMatch.Groups[1].Value);
                }
                
                var typeMatch = Regex.Match(match.Value, @"qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)");
                if (typeMatch.Success)
                {
                    field.Type = new SimpleTypeNode
                    {
                        Name = typeMatch.Groups[1].Value,
                        IsBuiltIn = IsBuiltInType(typeMatch.Groups[1].Value)
                    };
                }
                
                fields.Add(field);
            }
            
            return fields;
        }
        
        private List<PropertyDeclarationNode> ParseClassProperties(string classText)
        {
            var properties = new List<PropertyDeclarationNode>();
            
            string propPattern = @"classProperty\s+property\s*\([^)]+\)";
            var matches = Regex.Matches(classText, propPattern);
            
            foreach (Match match in matches)
            {
                var property = new PropertyDeclarationNode();
                
                var nameMatch = Regex.Match(match.Value, @"ident\s*(\w+)");
                if (nameMatch.Success)
                {
                    property.Name = nameMatch.Groups[1].Value;
                }
                
                var typeMatch = Regex.Match(match.Value, @"qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)");
                if (typeMatch.Success)
                {
                    property.Type = new SimpleTypeNode
                    {
                        Name = typeMatch.Groups[1].Value,
                        IsBuiltIn = IsBuiltInType(typeMatch.Groups[1].Value)
                    };
                }
                
                if (match.Value.Contains("read"))
                {
                    var readMatch = Regex.Match(match.Value, @"read\s*\(\s*qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)\s*\)");
                    if (readMatch.Success)
                    {
                        property.Getter = new PropertyAccessor
                        {
                            Name = readMatch.Groups[1].Value,
                            IsDirectField = true
                        };
                    }
                }
                
                if (match.Value.Contains("write"))
                {
                    var writeMatch = Regex.Match(match.Value, @"write\s*\(\s*qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)\s*\)");
                    if (writeMatch.Success)
                    {
                        property.Setter = new PropertyAccessor
                        {
                            Name = writeMatch.Groups[1].Value,
                            IsDirectField = true
                        };
                    }
                }
                
                properties.Add(property);
            }
            
            return properties;
        }
        
        private List<MethodDeclarationNode> ParseClassMethods(string classText)
        {
            var methods = new List<MethodDeclarationNode>();
            
            string methodPattern = @"classMethod\s*\([^)]+\)";
            var matches = Regex.Matches(classText, methodPattern);
            
            foreach (Match match in matches)
            {
                var method = new MethodDeclarationNode();
                
                if (match.Value.Contains("constructor"))
                    method.Kind = MethodKind.Constructor;
                else if (match.Value.Contains("destructor"))
                    method.Kind = MethodKind.Destructor;
                else if (match.Value.Contains("function"))
                    method.Kind = MethodKind.Function;
                else
                    method.Kind = MethodKind.Procedure;
                
                var nameMatch = Regex.Match(match.Value, @"ident\s*(\w+)");
                if (nameMatch.Success)
                {
                    method.Name = nameMatch.Groups[1].Value;
                }
                
                methods.Add(method);
            }
            
            return methods;
        }
        
        private void ParseGlobalDeclarations(string text, CompilationUnitNode unit)
        {
            // Parse global variables
            string varPattern = @"varSection\s*\([^)]+\)";
            var varMatches = Regex.Matches(text, varPattern, RegexOptions.Singleline);
            
            foreach (Match match in varMatches)
            {
                var declMatches = Regex.Matches(match.Value, @"identListFlat\s*\(\s*ident\s*(\w+)\s*\)");
                var typeMatches = Regex.Matches(match.Value, @"qualifiedIdent\s*\(\s*ident\s*(\w+)\s*\)");
                
                for (int i = 0; i < declMatches.Count; i++)
                {
                    var varNode = new VariableDeclarationNode
                    {
                        Names = new List<string> { declMatches[i].Groups[1].Value }
                    };
                    
                    if (i < typeMatches.Count)
                    {
                        varNode.Type = new SimpleTypeNode
                        {
                            Name = typeMatches[i].Groups[1].Value,
                            IsBuiltIn = IsBuiltInType(typeMatches[i].Groups[1].Value)
                        };
                    }
                    
                    unit.GlobalVariables.Add(varNode);
                }
            }
            
            // Parse constants
            string constPattern = @"constSection\s*\([^)]+\)";
            var constMatch = Regex.Match(text, constPattern, RegexOptions.Singleline);
            
            if (constMatch.Success)
            {
                var constMatches = Regex.Matches(constMatch.Value, @"ident\s*(\w+)\s*:\s*[^=]+=\s*stringFactor\s*'([^']*)'");
                
                foreach (Match match in constMatches)
                {
                    var constNode = new ConstantDeclarationNode
                    {
                        Name = match.Groups[1].Value,
                        Value = new LiteralNode
                        {
                            Value = match.Groups[2].Value,
                            LiteralType = "string"
                        }
                    };
                    
                    unit.Constants.Add(constNode);
                }
            }
        }
        
        private void ParseMethodsAndProcedures(string text, CompilationUnitNode unit)
        {
            // Parse procedures
            string procPattern = @"procDecl\s*\([^)]+\)";
            var procMatches = Regex.Matches(text, procPattern, RegexOptions.Singleline);
            
            foreach (Match match in procMatches)
            {
                var name = ExtractName(match.Value);
                if (!string.IsNullOrEmpty(name))
                {
                    var procNode = new ProcedureNode
                    {
                        Name = name,
                        Parameters = ParseParameters(match.Value)
                    };
                    
                    unit.Procedures.Add(procNode);
                }
            }
        }
        
        private string ExtractName(string text)
        {
            var match = Regex.Match(text, @"ident\s*(\w+)");
            return match.Success ? match.Groups[1].Value : "";
        }
        
        private List<ParameterNode> ParseParameters(string text)
        {
            var parameters = new List<ParameterNode>();
            
            var paramMatches = Regex.Matches(text, @"identListFlat\s*\(\s*ident\s*(\w+)\s*\)\s*:\s*[^;)]+");
            
            foreach (Match match in paramMatches)
            {
                var paramNode = new ParameterNode
                {
                    Names = new List<string> { match.Groups[1].Value }
                };
                
                parameters.Add(paramNode);
            }
            
            return parameters;
        }
        
        private bool IsBuiltInType(string typeName)
        {
            var builtInTypes = new HashSet<string>
            {
                "Integer", "String", "Boolean", "Char", "Real", "Double",
                "Single", "Extended", "Byte", "Word", "Cardinal", "Int64",
                "UInt64", "Pointer", "Variant", "TObject"
            };
            
            return builtInTypes.Contains(typeName);
        }
    }
}
