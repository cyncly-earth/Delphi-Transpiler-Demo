// // Simple test for AST - Doesn't affect main project
// using System;
// using System.Text.Json;

// public class SimpleAstTest
// {
//     public static void RunTest()
//     {
//         Console.WriteLine("=== Simple AST Test ===");
        
//         // Test data - simplified parse tree
//         string testParseTree = @"
//             (file (unit (unitHead unit (namespaceName (ident TestUnit)) ;)
//                 (unitInterface interface
//                     (usesClause uses (namespaceNameList (namespaceName (ident System)) ;))
//                     (interfaceDecl (typeSection type
//                         (typeDeclaration (genericTypeIdent (qualifiedIdent (ident TTest)))
//                             = (typeDecl (strucType (strucTypePart
//                                 (classDecl (classTypeDecl class
//                                     (classItem (classField (identList (ident Field1)) : 
//                                         (typeDecl (typeId (namespacedQualifiedIdent (qualifiedIdent (ident Integer))))) ;))
//                                     (classItem (classProperty property (ident Prop1) : 
//                                         (genericTypeIdent (qualifiedIdent (ident String))) 
//                                         (classPropertySpecifier (classPropertyReadWrite read (qualifiedIdent (ident Field1)))) ;))
//                                 end))
//                             ))) ;))
//                 )))
//             )";
        
//         try
//         {
//             var builder = new DelphiTranspiler.AST.AstBuilder();
//             var ast = builder.BuildFromParseTree(testParseTree, "TestUnit");
            
//             Console.WriteLine("✓ AST built successfully");
//             Console.WriteLine($"  Unit: {ast.Name}");
//             Console.WriteLine($"  Types: {ast.TypeDeclarations.Count}");
            
//             if (ast.TypeDeclarations.Count > 0)
//             {
//                 var typeDecl = ast.TypeDeclarations[0];
//                 Console.WriteLine($"  First type: {typeDecl.Name}");
                
//                 if (typeDecl.Type is DelphiTranspiler.AST.ClassTypeNode classType)
//                 {
//                     Console.WriteLine($"    Fields: {classType.ClassDefinition?.Fields.Count ?? 0}");
//                     Console.WriteLine($"    Properties: {classType.ClassDefinition?.Properties.Count ?? 0}");
//                 }
//             }
            
//             // Try to serialize
//             var json = JsonSerializer.Serialize(ast, new JsonSerializerOptions { WriteIndented = true });
//             Console.WriteLine($"✓ JSON serialization successful ({json.Length} bytes)");
            
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"✗ Error: {ex.Message}");
//         }
//     }
// }
