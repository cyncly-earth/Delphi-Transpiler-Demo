# Delphi Transpiler - Complete Workflow Explanation

## 📋 Overview

The Delphi Transpiler Demo is a **3-stage transpilation pipeline** that converts Delphi code into an Abstract Syntax Tree (AST) representation:

```
[Delphi Source Code] → [ANTLR Parser] → [Parse Tree] → [AST Builder] → [AST JSON]
```

---

## 🔧 Stage 1: Build Fixes (COMPLETED ✅)

### Issue Fixed
The project had **26 compilation errors** preventing the build.

### Root Causes & Solutions

#### 1. Multiple Entry Points Error
**Problem**: Both `Program.cs` and `ast_runner.cs` had `Main()` methods, causing CS0017 error.

**Solution**: Renamed `AstRunner.Main()` → `AstRunner.RunAst()` in [ast/ast_runner.cs](ast/ast_runner.cs#L13)
- Only one entry point per application allowed in C#
- `RunAst()` can be called manually if needed

#### 2. Abstract Type Instantiation Errors (18 occurrences)
**Problem**: Code tried to create instances of abstract classes:
```csharp
public TypeNode Type { get; set; } = new();  // ❌ ERROR: TypeNode is abstract
public ExpressionNode Value { get; set; } = new();  // ❌ ERROR: ExpressionNode is abstract
```

**Solution**: Changed to nullable types without default initializers in [ast/ast_nodes.cs](ast/ast_nodes.cs):
```csharp
public TypeNode? Type { get; set; }  // ✅ Nullable, no default
public ExpressionNode? Value { get; set; }  // ✅ Nullable, no default
```

**Affected Classes**:
- TypeDeclarationNode
- FieldDeclarationNode
- PropertyDeclarationNode
- ArrayTypeNode
- AssignmentStatementNode
- IfStatementNode
- ForStatementNode
- WhileStatementNode
- WithStatementNode
- CallStatementNode
- MemberAccessNode
- CallExpressionNode
- BinaryExpressionNode
- UnaryExpressionNode
- VariableDeclarationNode
- ConstantDeclarationNode
- ParameterNode
- FunctionNode

---

## 📊 Stage 2: Parsing (Delphi → Parse Tree)

### What Happens
[Program.cs](Program.cs) performs the **lexical and syntactic analysis**:

1. **Lexer**: Tokenizes the input stream
   - Reads character-by-character
   - Recognizes tokens (keywords, identifiers, operators)

2. **Parser**: Builds a parse tree from tokens
   - Follows grammar rules from [antlr/Delphi.g4](antlr/Delphi.g4)
   - Creates hierarchical tree structure matching grammar

3. **Output**: `.parse.txt` files (verbose, grammar-focused)
   ```
   (file (unit (unitHead unit (namespaceName (ident classPerson)) ;) ...))
   ```

### Grammar Limitations
Some Delphi syntax isn't recognized:
- `Array of` keyword in parameter declarations
- Missing semicolon handling

These cause **parser warnings** but don't stop processing.

---

## 🌳 Stage 3: AST Conversion (Parse Tree → AST)

### What Happens
[Program.cs](Program.cs) now calls [AstBuilder](ast/ast_builder.cs) to transform parse trees into semantic AST:

1. **Parse Extract**: Regex patterns extract meaningful information
   - Identifies class declarations
   - Extracts method signatures
   - Finds procedure parameters

2. **AST Construction**: Creates semantic nodes
   - `CompilationUnitNode` - represents entire unit
   - `ClassTypeNode` - represents classes
   - `ProcedureNode` - represents procedures
   - `MethodDeclarationNode` - represents methods

3. **JSON Serialization**: Converts AST to pretty-printed JSON
   ```json
   {
     "name": "classPerson",
     "typeDeclarations": [
       {
         "name": "TPerson",
         "nodeType": "TypeDeclarationNode"
       }
     ]
   }
   ```

### Key Differences: Parse Tree vs AST

| Aspect | Parse Tree | AST |
|--------|-----------|-----|
| **Grammar-Centric** | Yes (shows all rules) | No (only semantics) |
| **Size** | ~12-24 KB | ~400-2200 bytes |
| **Readability** | Very verbose | Semantic & clear |
| **Purpose** | Debug/validation | Transpilation |
| **Example** | `(classField (identList ...))` | `{ "nodeType": "FieldDeclarationNode" }` |

---

## 🚀 Complete Workflow Execution

### Command
```bash
dotnet run
```

### Output
```
=== STEP 1: PARSING DELPHI FILES TO PARSE TREES ===
Parsing: run/input/classPerson.pas
  -> Wrote parse output to: run/output/classPerson.parse.txt
Parsing: run/input/PersonController.pas
  [Parser warnings about Array of syntax]
  -> Wrote parse output to: run/output/PersonController.parse.txt
Parsing: run/input/PersonView.pas
  -> Wrote parse output to: run/output/PersonView.parse.txt

=== STEP 2: CONVERTING PARSE TREES TO AST ===
Building AST from: run/output/classPerson.parse.txt
  ✓ Created AST: run/output/classPerson.ast.json
  
Building AST from: run/output/PersonController.parse.txt
  ✓ Created AST: run/output/PersonController.ast.json
  
Building AST from: run/output/PersonView.parse.txt
  ✓ Created AST: run/output/PersonView.ast.json

=== CONVERSION COMPLETE ===
✓ Successful: 3
✗ Failed: 0
```

---

## 📁 Output Files Generated

### In `run/output/` directory:

```
classPerson.parse.txt         (12 KB)  - Parse tree (verbose)
classPerson.ast.json          (415 B)  - AST (semantic)
PersonController.parse.txt    (24 KB)  - Parse tree
PersonController.ast.json     (1.1 KB) - AST
PersonView.parse.txt          (22 KB)  - Parse tree
PersonView.ast.json           (2.2 KB) - AST
```

---

## 🏗️ Project Architecture

```
Delphi-Transpiler-Demo/
├── Program.cs                    ← Main entry point (orchestrates both stages)
├── antlr/
│   ├── Delphi.g4               ← ANTLR grammar definition
│   └── generated/              ← Auto-generated lexer/parser
├── ast/
│   ├── ast_nodes.cs            ← AST node definitions (fixed)
│   ├── ast_builder.cs          ← Parse tree → AST converter
│   ├── ast_runner.cs           ← Alternative runner (renamed Main→RunAst)
│   └── test_simple.cs
└── run/
    ├── input/                  ← Input .pas files
    │   ├── classPerson.pas
    │   ├── PersonController.pas
    │   └── PersonView.pas
    └── output/                 ← Generated parse trees & ASTs
        ├── *.parse.txt
        └── *.ast.json
```

---

## 🔄 Data Flow Example

### Input: `classPerson.pas`
```delphi
unit classPerson;
interface
  type
    TPerson = class
      private
        cID: Integer;
      public
        constructor Create(nPersonID: Integer);
    end;
```

### After Stage 2 (Parsing): `classPerson.parse.txt`
```
(file (unit (unitHead unit (namespaceName (ident classPerson)) ;) 
  (unitInterface interface 
    (typeDeclaration (genericTypeIdent (qualifiedIdent (ident TPerson))) = 
      (typeDecl (strucType (strucTypePart (classDecl ...)))))))
```

### After Stage 3 (AST Conversion): `classPerson.ast.json`
```json
{
  "name": "classPerson",
  "unitType": "unit",
  "typeDeclarations": [
    {
      "name": "TPerson",
      "nodeType": "TypeDeclarationNode"
    }
  ],
  "nodeType": "CompilationUnitNode"
}
```

---

## ✅ Next Steps

The pipeline is now **fully functional**. You can:

1. **Add more Delphi source files** to `run/input/`
2. **Run the transpiler**: `dotnet run`
3. **Use the AST JSON** for code generation/transformation
4. **Improve grammar** in [Delphi.g4](antlr/Delphi.g4) to handle more syntax
5. **Enhance AST builder** in [ast_builder.cs](ast/ast_builder.cs) for deeper semantic extraction

---

## 📚 Key Classes & Files

### Core Execution
- [Program.cs](Program.cs) - Orchestrates parsing & AST conversion (modified)
- [ast/ast_runner.cs](ast/ast_runner.cs) - Alternative runner (renamed Main method)

### AST Definition
- [ast/ast_nodes.cs](ast/ast_nodes.cs) - All AST node class definitions (fixed)

### Conversion Logic
- [ast/ast_builder.cs](ast/ast_builder.cs) - Parse tree → AST transformation logic

### Grammar & Generated Code
- [antlr/Delphi.g4](antlr/Delphi.g4) - ANTLR grammar (source of parser)
- [antlr/generated/](antlr/generated/) - Auto-generated DelphiLexer, DelphiParser

---

## 🎯 Summary

| Stage | Input | Process | Output | Status |
|-------|-------|---------|--------|--------|
| **Build** | Source code | Fix compilation errors | Runnable DLL | ✅ Complete |
| **Parse** | `.pas` files | ANTLR lexer/parser | `.parse.txt` | ✅ Working |
| **AST** | `.parse.txt` | Regex extraction + serialization | `.ast.json` | ✅ Working |

All three stages are now **fully integrated and operational**! 🚀
