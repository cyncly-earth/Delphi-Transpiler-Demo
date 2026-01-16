# AST Builder Migration Guide - From Old Model to New Model

## Overview

Your single `NewAstBuilder` has been refactored to work with the new `AstModel.cs` structure that includes:
- **Interface/Implementation separation**
- **Structured parameters** (not strings)
- **Polymorphic statements** in procedure bodies
- **Proper visibility and field grouping**

---

## Key Changes

### 1. **Old vs New Structure**

#### OLD (Your Current Model)
```csharp
var unit = new AstUnit();
unit.Name = "classPerson";
unit.Classes = new List<AstClass> { ... };
unit.Procedures = new List<AstProcedure> { ... };
unit.Fields = new List<AstField> { ... };
```

#### NEW (Updated Model)
```csharp
var unit = new AstUnit
{
    Name = "classPerson",
    InterfaceSection = new AstSection
    {
        Uses = new List<string> { "SysUtils" },
        Classes = new List<AstClass> { ... },
        Procedures = new List<AstProcedure> { }, // Forward declarations
        Variables = new List<AstVarDeclaration> { }
    },
    ImplementationSection = new AstSection
    {
        Procedures = new List<AstProcedure> { ... } // Full implementations
    }
};
```

---

### 2. **Parameter Handling**

#### OLD
```csharp
var procedure = new AstProcedure
{
    Name = "Create",
    Parameters = "nPersonID: Integer; nClient: Integer" // ❌ String!
};
```

#### NEW
```csharp
var procedure = new AstProcedure
{
    Name = "Create",
    ProcedureType = "constructor",
    Parameters = new List<AstParameter>
    {
        new AstParameter 
        { 
            Names = new List<string> { "nPersonID" },
            ParameterType = "",      // var/const/out
            TypeName = "Integer",
            DefaultValue = ""
        },
        new AstParameter 
        { 
            Names = new List<string> { "nClient" },
            ParameterType = "",
            TypeName = "Integer",
            DefaultValue = ""
        }
    }
};
```

---

### 3. **Field Handling**

#### OLD
```csharp
var field = new AstField
{
    Name = "cID",           // ❌ Single name
    Type = "Integer"
};
```

#### NEW
```csharp
var field = new AstField
{
    Names = new List<string> { "cID" },  // ✅ Can be multiple
    TypeName = "Integer",
    Visibility = "private",              // ✅ Visibility tracked
    Span = new SourceSpan()
};
```

---

### 4. **Procedure Body**

#### OLD
```csharp
var procedure = new AstProcedure
{
    Body = "cID := nPersonID; cClient := nClient;" // ❌ String!
};
```

#### NEW
```csharp
var procedure = new AstProcedure
{
    Body = new List<AstStatement>
    {
        new AstAssignment 
        { 
            Target = "cID", 
            Value = "nPersonID",
            Span = new SourceSpan()
        },
        new AstAssignment 
        { 
            Target = "cClient", 
            Value = "nClient",
            Span = new SourceSpan()
        }
    }
};
```

---

### 5. **Class Properties**

#### NEW Addition
```csharp
var astClass = new AstClass
{
    Name = "TPerson",
    ClassType = "class",
    ParentTypes = new List<string>(),      // ✅ New: Inheritance tracking
    Properties = new List<AstProperty>     // ✅ New: Properties separated
    {
        new AstProperty
        {
            Name = "PersonID",
            TypeName = "Integer",
            ReadSpecifier = "cID",
            WriteSpecifier = "cID",
            Visibility = "public",
            Span = new SourceSpan()
        }
    }
};
```

---

## Refactored Builder Methods

### Method Mapping

| Old Method | New Method | Change |
|---|---|---|
| `ExtractClasses()` | `ExtractClasses()` | ✅ Enhanced: Now extracts properties and parent types |
| `ExtractClassFields()` | `ExtractClassFields()` | ✅ Enhanced: Now tracks visibility |
| `ExtractClassMethods()` | `ExtractClassMethods()` | ✅ Enhanced: Properly structured parameters |
| `ExtractProcedures()` | `ExtractModuleProcedures()` | ✅ Enhanced: Separates interface/implementation |
| `ExtractGlobalFields()` | `ExtractGlobalVariables()` | ✅ Renamed: Now returns AstVarDeclaration |
| NEW | `ExtractUsesClause()` | ✅ New: Parses use/import statements |
| NEW | `ExtractClassProperties()` | ✅ New: Separate property extraction |
| NEW | `ExtractProcedureParameters()` | ✅ New: Structured parameter extraction |
| NEW | `ExtractProcedureLocalVariables()` | ✅ New: Local variable extraction |
| NEW | `ExtractProcedureBody()` | ✅ New: Statement extraction |
| NEW | `ExtractStatementsFromBlock()` | ✅ New: Polymorphic statement parsing |

---

## Usage Example

### Input: classPerson.pas
```delphi
unit classPerson;

interface

type
  TPerson = class
  private
    cID     : Integer;
    cClient : Integer;
  public
    property PersonID : Integer read cID write cID;
    constructor Create(nPersonID : Integer; nClient : Integer);
    function ToString : String;
  end;

implementation

constructor TPerson.Create(nPersonID : Integer; nClient : Integer);
begin
  cID     := nPersonID;
  cClient := nClient;
end;

function TPerson.ToString : String;
begin
  Result := cID;
end;

end.
```

### Code Usage
```csharp
var builder = new NewAstBuilder();
var parseTreeText = File.ReadAllText("parseTree.txt");
var unit = builder.BuildFromParseTree(parseTreeText, "classPerson.pas");

// Serialize to JSON
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
string json = JsonSerializer.Serialize(unit, options);
File.WriteAllText("classPerson.ast.json", json);
```

### Output Structure
```
AstUnit: classPerson
├── InterfaceSection
│   ├── Uses: []
│   └── Classes[0]
│       └── TPerson
│           ├── Fields[]: [cID, cClient] (private)
│           ├── Properties[]: [PersonID]
│           └── Methods[]: [Create (constructor), ToString (function)]
└── ImplementationSection
    └── Procedures[]: [TPerson.Create, TPerson.ToString]
        └── Body: [AstAssignment, AstAssignment, ...]
```

---

## Three-File Processing

Your builder is already designed to handle multiple files simultaneously:

```csharp
var files = new[] 
{ 
    "classPerson.pas",
    "PersonView.pas", 
    "PersonController.pas" 
};

foreach (var file in files)
{
    var parseTree = ParseDelphiFile(file);
    var unit = builder.BuildFromParseTree(parseTree, file);
    var json = JsonSerializer.Serialize(unit, options);
    File.WriteAllText($"{Path.GetFileNameWithoutExtension(file)}.ast.json", json);
}
```

---

## Output JSON Format

Each file produces a corresponding JSON file following this schema:

```json
{
  "name": "classPerson",
  "interfaceSection": {
    "uses": ["SysUtils"],
    "constants": [],
    "types": [],
    "variables": [],
    "classes": [
      {
        "name": "TPerson",
        "classType": "class",
        "parentTypes": [],
        "fields": [
          {
            "names": ["cID"],
            "typeName": "Integer",
            "visibility": "private",
            "span": { "startLine": 6, "startColumn": 5, "endLine": 6, "endColumn": 23 }
          }
        ],
        "methods": [...],
        "properties": [...]
      }
    ],
    "procedures": []
  },
  "implementationSection": {
    "uses": [],
    "constants": [],
    "types": [],
    "variables": [],
    "classes": [],
    "procedures": [
      {
        "name": "TPerson.Create",
        "procedureType": "constructor",
        "parameters": [...],
        "returnType": "",
        "body": [
          { "$type": "assignment", "target": "cID", "value": "nPersonID" }
        ]
      }
    ]
  },
  "initializationStatements": [],
  "finalizationStatements": []
}
```

---

## Integration with Semantic Analysis

After AST generation, you can feed these ASTs into semantic analysis:

```csharp
// Load all AST files
var astDir = "output/ast";
var astFiles = Directory.GetFiles(astDir, "*.json");

foreach (var astFile in astFiles)
{
    var json = File.ReadAllText(astFile);
    var unit = JsonSerializer.Deserialize<AstUnit>(json, options);
    
    // Run semantic analysis
    var semantic = new BookingSemanticAnalyzer();
    var context = semantic.Analyze(unit);
    
    // Generate code
    var codegen = new DotnetCodeGenerator();
    var csCode = codegen.Generate(context);
}
```

---

## Testing Your Builder

Create a test to validate the refactored builder:

```csharp
[Test]
public void TestPersonClassParsing()
{
    var builder = new NewAstBuilder();
    var parseTree = File.ReadAllText("tests/classPerson.parse.txt");
    
    var unit = builder.BuildFromParseTree(parseTree, "classPerson.pas");
    
    Assert.AreEqual("classPerson", unit.Name);
    Assert.AreEqual(1, unit.InterfaceSection.Classes.Count);
    
    var tperson = unit.InterfaceSection.Classes[0];
    Assert.AreEqual("TPerson", tperson.Name);
    Assert.AreEqual(5, tperson.Fields.Count);
    Assert.AreEqual(5, tperson.Properties.Count);
    Assert.AreEqual(2, tperson.Methods.Count);
}
```

---

## Summary of Improvements

✅ **Interface/Implementation separation** - Proper Delphi semantics  
✅ **Structured parameters** - Type-safe, JSON serializable  
✅ **Polymorphic statements** - Rich program representation  
✅ **Properties handling** - Separate from fields  
✅ **Visibility tracking** - public/private/protected  
✅ **Source location tracking** - SourceSpan for error reporting  
✅ **JSON compatibility** - Full camelCase serialization  
✅ **Forward declarations** - Proper interface/implementation split  

---

## Next Steps

1. **Generate parse trees** from your Delphi files using ANTLR
2. **Run the builder** on each parse tree
3. **Serialize to JSON** and validate output
4. **Feed into semantic analysis** for code generation
5. **Generate C# + TypeScript** output

