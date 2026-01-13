# Generated from MyGrammar.g4 by ANTLR 4.13.2
from antlr4 import *
if "." in __name__:
    from .MyGrammarParser import MyGrammarParser
else:
    from MyGrammarParser import MyGrammarParser

# This class defines a complete generic visitor for a parse tree produced by MyGrammarParser.

class MyGrammarVisitor(ParseTreeVisitor):

    # Visit a parse tree produced by MyGrammarParser#program.
    def visitProgram(self, ctx:MyGrammarParser.ProgramContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#programHeading.
    def visitProgramHeading(self, ctx:MyGrammarParser.ProgramHeadingContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#identifier.
    def visitIdentifier(self, ctx:MyGrammarParser.IdentifierContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#block.
    def visitBlock(self, ctx:MyGrammarParser.BlockContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#usesUnitsPart.
    def visitUsesUnitsPart(self, ctx:MyGrammarParser.UsesUnitsPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#labelDeclarationPart.
    def visitLabelDeclarationPart(self, ctx:MyGrammarParser.LabelDeclarationPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#label.
    def visitLabel(self, ctx:MyGrammarParser.LabelContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#constantDefinitionPart.
    def visitConstantDefinitionPart(self, ctx:MyGrammarParser.ConstantDefinitionPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#constantDefinition.
    def visitConstantDefinition(self, ctx:MyGrammarParser.ConstantDefinitionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#constantChr.
    def visitConstantChr(self, ctx:MyGrammarParser.ConstantChrContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#constant.
    def visitConstant(self, ctx:MyGrammarParser.ConstantContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unsignedNumber.
    def visitUnsignedNumber(self, ctx:MyGrammarParser.UnsignedNumberContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unsignedInteger.
    def visitUnsignedInteger(self, ctx:MyGrammarParser.UnsignedIntegerContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unsignedReal.
    def visitUnsignedReal(self, ctx:MyGrammarParser.UnsignedRealContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#sign.
    def visitSign(self, ctx:MyGrammarParser.SignContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#bool_.
    def visitBool_(self, ctx:MyGrammarParser.Bool_Context):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#string.
    def visitString(self, ctx:MyGrammarParser.StringContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#typeDefinitionPart.
    def visitTypeDefinitionPart(self, ctx:MyGrammarParser.TypeDefinitionPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#typeDefinition.
    def visitTypeDefinition(self, ctx:MyGrammarParser.TypeDefinitionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#functionType.
    def visitFunctionType(self, ctx:MyGrammarParser.FunctionTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#procedureType.
    def visitProcedureType(self, ctx:MyGrammarParser.ProcedureTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#type_.
    def visitType_(self, ctx:MyGrammarParser.Type_Context):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#simpleType.
    def visitSimpleType(self, ctx:MyGrammarParser.SimpleTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#scalarType.
    def visitScalarType(self, ctx:MyGrammarParser.ScalarTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#subrangeType.
    def visitSubrangeType(self, ctx:MyGrammarParser.SubrangeTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#typeIdentifier.
    def visitTypeIdentifier(self, ctx:MyGrammarParser.TypeIdentifierContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#structuredType.
    def visitStructuredType(self, ctx:MyGrammarParser.StructuredTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unpackedStructuredType.
    def visitUnpackedStructuredType(self, ctx:MyGrammarParser.UnpackedStructuredTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#stringtype.
    def visitStringtype(self, ctx:MyGrammarParser.StringtypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#arrayType.
    def visitArrayType(self, ctx:MyGrammarParser.ArrayTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#typeList.
    def visitTypeList(self, ctx:MyGrammarParser.TypeListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#indexType.
    def visitIndexType(self, ctx:MyGrammarParser.IndexTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#componentType.
    def visitComponentType(self, ctx:MyGrammarParser.ComponentTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#recordType.
    def visitRecordType(self, ctx:MyGrammarParser.RecordTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#fieldList.
    def visitFieldList(self, ctx:MyGrammarParser.FieldListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#fixedPart.
    def visitFixedPart(self, ctx:MyGrammarParser.FixedPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#recordSection.
    def visitRecordSection(self, ctx:MyGrammarParser.RecordSectionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#variantPart.
    def visitVariantPart(self, ctx:MyGrammarParser.VariantPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#tag.
    def visitTag(self, ctx:MyGrammarParser.TagContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#variant.
    def visitVariant(self, ctx:MyGrammarParser.VariantContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#setType.
    def visitSetType(self, ctx:MyGrammarParser.SetTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#baseType.
    def visitBaseType(self, ctx:MyGrammarParser.BaseTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#fileType.
    def visitFileType(self, ctx:MyGrammarParser.FileTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#pointerType.
    def visitPointerType(self, ctx:MyGrammarParser.PointerTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#variableDeclarationPart.
    def visitVariableDeclarationPart(self, ctx:MyGrammarParser.VariableDeclarationPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#variableDeclaration.
    def visitVariableDeclaration(self, ctx:MyGrammarParser.VariableDeclarationContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#procedureAndFunctionDeclarationPart.
    def visitProcedureAndFunctionDeclarationPart(self, ctx:MyGrammarParser.ProcedureAndFunctionDeclarationPartContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#procedureOrFunctionDeclaration.
    def visitProcedureOrFunctionDeclaration(self, ctx:MyGrammarParser.ProcedureOrFunctionDeclarationContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#procedureDeclaration.
    def visitProcedureDeclaration(self, ctx:MyGrammarParser.ProcedureDeclarationContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#formalParameterList.
    def visitFormalParameterList(self, ctx:MyGrammarParser.FormalParameterListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#formalParameterSection.
    def visitFormalParameterSection(self, ctx:MyGrammarParser.FormalParameterSectionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#parameterGroup.
    def visitParameterGroup(self, ctx:MyGrammarParser.ParameterGroupContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#identifierList.
    def visitIdentifierList(self, ctx:MyGrammarParser.IdentifierListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#constList.
    def visitConstList(self, ctx:MyGrammarParser.ConstListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#functionDeclaration.
    def visitFunctionDeclaration(self, ctx:MyGrammarParser.FunctionDeclarationContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#resultType.
    def visitResultType(self, ctx:MyGrammarParser.ResultTypeContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#statement.
    def visitStatement(self, ctx:MyGrammarParser.StatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unlabelledStatement.
    def visitUnlabelledStatement(self, ctx:MyGrammarParser.UnlabelledStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#simpleStatement.
    def visitSimpleStatement(self, ctx:MyGrammarParser.SimpleStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#assignmentStatement.
    def visitAssignmentStatement(self, ctx:MyGrammarParser.AssignmentStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#variable.
    def visitVariable(self, ctx:MyGrammarParser.VariableContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#expression.
    def visitExpression(self, ctx:MyGrammarParser.ExpressionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#relationaloperator.
    def visitRelationaloperator(self, ctx:MyGrammarParser.RelationaloperatorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#simpleExpression.
    def visitSimpleExpression(self, ctx:MyGrammarParser.SimpleExpressionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#additiveoperator.
    def visitAdditiveoperator(self, ctx:MyGrammarParser.AdditiveoperatorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#term.
    def visitTerm(self, ctx:MyGrammarParser.TermContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#multiplicativeoperator.
    def visitMultiplicativeoperator(self, ctx:MyGrammarParser.MultiplicativeoperatorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#signedFactor.
    def visitSignedFactor(self, ctx:MyGrammarParser.SignedFactorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#factor.
    def visitFactor(self, ctx:MyGrammarParser.FactorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#unsignedConstant.
    def visitUnsignedConstant(self, ctx:MyGrammarParser.UnsignedConstantContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#functionDesignator.
    def visitFunctionDesignator(self, ctx:MyGrammarParser.FunctionDesignatorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#parameterList.
    def visitParameterList(self, ctx:MyGrammarParser.ParameterListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#set_.
    def visitSet_(self, ctx:MyGrammarParser.Set_Context):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#elementList.
    def visitElementList(self, ctx:MyGrammarParser.ElementListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#element.
    def visitElement(self, ctx:MyGrammarParser.ElementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#procedureStatement.
    def visitProcedureStatement(self, ctx:MyGrammarParser.ProcedureStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#actualParameter.
    def visitActualParameter(self, ctx:MyGrammarParser.ActualParameterContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#parameterwidth.
    def visitParameterwidth(self, ctx:MyGrammarParser.ParameterwidthContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#gotoStatement.
    def visitGotoStatement(self, ctx:MyGrammarParser.GotoStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#emptyStatement_.
    def visitEmptyStatement_(self, ctx:MyGrammarParser.EmptyStatement_Context):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#empty_.
    def visitEmpty_(self, ctx:MyGrammarParser.Empty_Context):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#structuredStatement.
    def visitStructuredStatement(self, ctx:MyGrammarParser.StructuredStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#compoundStatement.
    def visitCompoundStatement(self, ctx:MyGrammarParser.CompoundStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#statements.
    def visitStatements(self, ctx:MyGrammarParser.StatementsContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#conditionalStatement.
    def visitConditionalStatement(self, ctx:MyGrammarParser.ConditionalStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#ifStatement.
    def visitIfStatement(self, ctx:MyGrammarParser.IfStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#caseStatement.
    def visitCaseStatement(self, ctx:MyGrammarParser.CaseStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#caseListElement.
    def visitCaseListElement(self, ctx:MyGrammarParser.CaseListElementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#repetetiveStatement.
    def visitRepetetiveStatement(self, ctx:MyGrammarParser.RepetetiveStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#whileStatement.
    def visitWhileStatement(self, ctx:MyGrammarParser.WhileStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#repeatStatement.
    def visitRepeatStatement(self, ctx:MyGrammarParser.RepeatStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#forStatement.
    def visitForStatement(self, ctx:MyGrammarParser.ForStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#forList.
    def visitForList(self, ctx:MyGrammarParser.ForListContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#initialValue.
    def visitInitialValue(self, ctx:MyGrammarParser.InitialValueContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#finalValue.
    def visitFinalValue(self, ctx:MyGrammarParser.FinalValueContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#withStatement.
    def visitWithStatement(self, ctx:MyGrammarParser.WithStatementContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by MyGrammarParser#recordVariableList.
    def visitRecordVariableList(self, ctx:MyGrammarParser.RecordVariableListContext):
        return self.visitChildren(ctx)



del MyGrammarParser