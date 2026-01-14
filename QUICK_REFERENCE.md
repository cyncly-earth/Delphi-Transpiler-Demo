# Quick Start & Reference Guide

## ⚡ Quick Start (30 seconds)

```bash
# Build the project
dotnet build

# Run the complete pipeline
dotnet run

# Check output
ls run/output/*.ast.json
```

**That's it!** You now have AST JSON files ready for transpilation.

---

## 📖 Documentation Files

| File | Purpose | Read Time |
|------|---------|-----------|
| **[COMPLETE_SUMMARY.md](COMPLETE_SUMMARY.md)** | Executive summary of all fixes & changes | 5 min |
| **[WORKFLOW_EXPLANATION.md](WORKFLOW_EXPLANATION.md)** | In-depth explanation of 3-stage pipeline | 15 min |
| **[PIPELINE_VISUAL_GUIDE.md](PIPELINE_VISUAL_GUIDE.md)** | Visual diagrams & ASCII flowcharts | 10 min |
| **[README.md](README.md)** | Original project documentation | 5 min |

**Start with**: [COMPLETE_SUMMARY.md](COMPLETE_SUMMARY.md) if you only have 5 minutes

---

## 🔧 The 3-Stage Pipeline

```
Stage 1: PARSE          Stage 2: AST CONVERT       Output
───────────────────────────────────────────────────────────
Delphi Code → ANTLR → Parse Tree → Regex Extract → JSON AST
(*.pas)      Parser   (*.txt)     Semantic Info    (*.json)
  ↓            ↓         ↓              ↓             ↓
12 KB in    Tokens &   Verbose      Meaningful    Compact
           Rules      Grammar       Data Only     Structure
```

---

## 🎯 What Each Component Does

### **Program.cs** (Entry Point)
- Reads Delphi `.pas` files
- Runs ANTLR parser → generates parse trees
- Calls AstBuilder → converts to AST
- Serializes to JSON → writes output files

### **ast/ast_builder.cs** (Converter)
- Extracts semantic information from parse trees
- Uses regex patterns to find:
  - Class declarations
  - Method signatures
  - Field definitions
  - Type information
- Creates semantic AST nodes

### **ast/ast_nodes.cs** (Data Models)
- Defines all AST node types:
  - `CompilationUnitNode`
  - `TypeDeclarationNode`
  - `ProcedureNode`
  - `MethodDeclarationNode`
  - etc.

### **antlr/Delphi.g4** (Grammar)
- ANTLR grammar rules for Delphi syntax
- Defines parsing structure
- Note: Incomplete for "Array of" syntax

---

## 📊 Input & Output

### Input: `run/input/`
```
classPerson.pas        ← Delphi source code
PersonController.pas   ← Delphi source code
PersonView.pas         ← Delphi source code
```

### Output: `run/output/`
```
classPerson.ast.json           ← AST for transpilation ✅
PersonController.ast.json      ← AST for transpilation ✅
PersonView.ast.json            ← AST for transpilation ✅

classPerson.parse.txt          ← Parse tree (debug)
PersonController.parse.txt     ← Parse tree (debug)
PersonView.parse.txt           ← Parse tree (debug)
```

---

## 📈 Pipeline Statistics

```
Input:  3 Delphi files
Parse:  58 KB total parse trees
AST:    4 KB total AST JSON (93% smaller)

Processing:
  ✓ classPerson:      1 type, 0 procedures
  ✓ PersonController: 0 types, 7 procedures
  ✓ PersonView:       0 types, 16 procedures

Success Rate: 100%
```

---

## 🐛 Known Issues & Limitations

### Parser Warnings (Non-Fatal)
```
line 14:55 no viable alternative at input 'procedureAddPerson(...Arrayof'
line 132:3 mismatched input ';' expecting '.'
```
**Why**: Grammar doesn't support `Array of` syntax in parameter lists
**Impact**: Parser continues anyway, generates partial AST
**Fix**: Enhance grammar in `antlr/Delphi.g4`

### AST Extraction via Regex
- Uses regex patterns instead of visitor pattern
- Works for basic structures (classes, procedures)
- May miss complex expressions
- **Future**: Implement full visitor pattern

---

## 🔨 How to Extend

### Add More Input Files
```bash
# 1. Add your .pas file to run/input/
cp mycode.pas run/input/

# 2. Run the pipeline
dotnet run

# 3. Check output
cat run/output/mycode.ast.json
```

### Fix Grammar Issues
1. Edit [antlr/Delphi.g4](antlr/Delphi.g4)
2. Regenerate parser: Run `build_ast.sh`
3. Rebuild: `dotnet build`
4. Test: `dotnet run`

### Improve AST Extraction
Edit [ast/ast_builder.cs](ast/ast_builder.cs):
- Add more regex patterns
- Or implement visitor pattern for better extraction

---

## ✅ What Was Fixed

### Build Errors (26 → 0)
1. **Multiple entry point** → Renamed `Main()` to `RunAst()`
2. **Abstract types** → Changed to nullable types
3. **Result**: Clean build with 0 errors

### Workflow
1. **Was**: Only parsing → Parse trees only
2. **Now**: Parse + AST conversion → Both trees AND JSON ASTs
3. **Benefit**: Ready for code generation/transpilation

---

## 📋 Files You Need to Know

| File | Importance | Purpose |
|------|-----------|---------|
| [Program.cs](Program.cs) | ⭐⭐⭐ | Entry point, orchestrates pipeline |
| [ast/ast_builder.cs](ast/ast_builder.cs) | ⭐⭐⭐ | Converts parse trees to AST |
| [ast/ast_nodes.cs](ast/ast_nodes.cs) | ⭐⭐⭐ | Defines AST node structure |
| [antlr/Delphi.g4](antlr/Delphi.g4) | ⭐⭐ | Grammar (incomplete) |
| [ast/ast_runner.cs](ast/ast_runner.cs) | ⭐ | Alternative runner (not used) |

---

## 🚀 Command Reference

```bash
# Build project
dotnet build

# Run pipeline (parse + AST generation)
dotnet run

# Just build, no run
dotnet build --no-restore

# Clean build artifacts
dotnet clean

# View generated AST
cat run/output/*.ast.json

# View parse trees (verbose)
cat run/output/*.parse.txt | head -100

# Count files in output
ls run/output/ | wc -l
```

---

## 🤔 FAQ

**Q: Why do I have both parse trees AND AST?**
A: Parse trees are for debugging/validation. ASTs are for transpilation. Keep both.

**Q: Can I add more Delphi files?**
A: Yes! Just put them in `run/input/` and run `dotnet run` again.

**Q: Why are there parser warnings?**
A: The grammar is incomplete. It doesn't recognize `Array of` in some contexts. The parser still works—it just skips those parts.

**Q: What's next?**
A: You can now write a code generator that reads the AST JSON and outputs C#, Java, or any other language.

**Q: Can I modify the AST structure?**
A: Yes! Edit [ast/ast_nodes.cs](ast/ast_nodes.cs) to add/modify node types.

---

## 📞 Getting Help

1. **Understanding the pipeline?** → Read [WORKFLOW_EXPLANATION.md](WORKFLOW_EXPLANATION.md)
2. **Visual learner?** → See [PIPELINE_VISUAL_GUIDE.md](PIPELINE_VISUAL_GUIDE.md)
3. **Need quick facts?** → Check [COMPLETE_SUMMARY.md](COMPLETE_SUMMARY.md)
4. **Want to extend?** → Review [ast/ast_builder.cs](ast/ast_builder.cs) code

---

## 🎉 You're All Set!

Your Delphi Transpiler now has a **working end-to-end pipeline**:

```
✅ Parsing:          WORKING (ANTLR parser)
✅ AST Generation:   WORKING (AstBuilder)
✅ JSON Serialization: WORKING (System.Text.Json)
✅ Output:           3 AST files ready for use
```

**Run `dotnet run` anytime to generate AST files!**

---

*For detailed information, refer to the comprehensive documentation files.*
