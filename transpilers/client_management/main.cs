using DelphiTranspiler.Ast;

var builder = new PersonAstBuilder();
// Path relative to your workspace root
builder.Build("./run/input/PersonController.pas", "./output");