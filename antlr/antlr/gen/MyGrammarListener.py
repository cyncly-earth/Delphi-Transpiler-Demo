# Generated from MyGrammar.g4 by ANTLR 4.13.2
from antlr4 import *
if "." in __name__:
    from .MyGrammarParser import MyGrammarParser
else:
    from MyGrammarParser import MyGrammarParser

# This class defines a complete listener for a parse tree produced by MyGrammarParser.
class MyGrammarListener(ParseTreeListener):

    # Enter a parse tree produced by MyGrammarParser#program.
    def enterProgram(self, ctx:MyGrammarParser.ProgramContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#program.
    def exitProgram(self, ctx:MyGrammarParser.ProgramContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#programHeading.
    def enterProgramHeading(self, ctx:MyGrammarParser.ProgramHeadingContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#programHeading.
    def exitProgramHeading(self, ctx:MyGrammarParser.ProgramHeadingContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#identifier.
    def enterIdentifier(self, ctx:MyGrammarParser.IdentifierContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#identifier.
    def exitIdentifier(self, ctx:MyGrammarParser.IdentifierContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#block.
    def enterBlock(self, ctx:MyGrammarParser.BlockContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#block.
    def exitBlock(self, ctx:MyGrammarParser.BlockContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#usesUnitsPart.
    def enterUsesUnitsPart(self, ctx:MyGrammarParser.UsesUnitsPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#usesUnitsPart.
    def exitUsesUnitsPart(self, ctx:MyGrammarParser.UsesUnitsPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#labelDeclarationPart.
    def enterLabelDeclarationPart(self, ctx:MyGrammarParser.LabelDeclarationPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#labelDeclarationPart.
    def exitLabelDeclarationPart(self, ctx:MyGrammarParser.LabelDeclarationPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#label.
    def enterLabel(self, ctx:MyGrammarParser.LabelContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#label.
    def exitLabel(self, ctx:MyGrammarParser.LabelContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#constantDefinitionPart.
    def enterConstantDefinitionPart(self, ctx:MyGrammarParser.ConstantDefinitionPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#constantDefinitionPart.
    def exitConstantDefinitionPart(self, ctx:MyGrammarParser.ConstantDefinitionPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#constantDefinition.
    def enterConstantDefinition(self, ctx:MyGrammarParser.ConstantDefinitionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#constantDefinition.
    def exitConstantDefinition(self, ctx:MyGrammarParser.ConstantDefinitionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#constantChr.
    def enterConstantChr(self, ctx:MyGrammarParser.ConstantChrContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#constantChr.
    def exitConstantChr(self, ctx:MyGrammarParser.ConstantChrContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#constant.
    def enterConstant(self, ctx:MyGrammarParser.ConstantContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#constant.
    def exitConstant(self, ctx:MyGrammarParser.ConstantContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unsignedNumber.
    def enterUnsignedNumber(self, ctx:MyGrammarParser.UnsignedNumberContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unsignedNumber.
    def exitUnsignedNumber(self, ctx:MyGrammarParser.UnsignedNumberContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unsignedInteger.
    def enterUnsignedInteger(self, ctx:MyGrammarParser.UnsignedIntegerContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unsignedInteger.
    def exitUnsignedInteger(self, ctx:MyGrammarParser.UnsignedIntegerContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unsignedReal.
    def enterUnsignedReal(self, ctx:MyGrammarParser.UnsignedRealContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unsignedReal.
    def exitUnsignedReal(self, ctx:MyGrammarParser.UnsignedRealContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#sign.
    def enterSign(self, ctx:MyGrammarParser.SignContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#sign.
    def exitSign(self, ctx:MyGrammarParser.SignContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#bool_.
    def enterBool_(self, ctx:MyGrammarParser.Bool_Context):
        pass

    # Exit a parse tree produced by MyGrammarParser#bool_.
    def exitBool_(self, ctx:MyGrammarParser.Bool_Context):
        pass


    # Enter a parse tree produced by MyGrammarParser#string.
    def enterString(self, ctx:MyGrammarParser.StringContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#string.
    def exitString(self, ctx:MyGrammarParser.StringContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#typeDefinitionPart.
    def enterTypeDefinitionPart(self, ctx:MyGrammarParser.TypeDefinitionPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#typeDefinitionPart.
    def exitTypeDefinitionPart(self, ctx:MyGrammarParser.TypeDefinitionPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#typeDefinition.
    def enterTypeDefinition(self, ctx:MyGrammarParser.TypeDefinitionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#typeDefinition.
    def exitTypeDefinition(self, ctx:MyGrammarParser.TypeDefinitionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#functionType.
    def enterFunctionType(self, ctx:MyGrammarParser.FunctionTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#functionType.
    def exitFunctionType(self, ctx:MyGrammarParser.FunctionTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#procedureType.
    def enterProcedureType(self, ctx:MyGrammarParser.ProcedureTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#procedureType.
    def exitProcedureType(self, ctx:MyGrammarParser.ProcedureTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#type_.
    def enterType_(self, ctx:MyGrammarParser.Type_Context):
        pass

    # Exit a parse tree produced by MyGrammarParser#type_.
    def exitType_(self, ctx:MyGrammarParser.Type_Context):
        pass


    # Enter a parse tree produced by MyGrammarParser#simpleType.
    def enterSimpleType(self, ctx:MyGrammarParser.SimpleTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#simpleType.
    def exitSimpleType(self, ctx:MyGrammarParser.SimpleTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#scalarType.
    def enterScalarType(self, ctx:MyGrammarParser.ScalarTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#scalarType.
    def exitScalarType(self, ctx:MyGrammarParser.ScalarTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#subrangeType.
    def enterSubrangeType(self, ctx:MyGrammarParser.SubrangeTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#subrangeType.
    def exitSubrangeType(self, ctx:MyGrammarParser.SubrangeTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#typeIdentifier.
    def enterTypeIdentifier(self, ctx:MyGrammarParser.TypeIdentifierContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#typeIdentifier.
    def exitTypeIdentifier(self, ctx:MyGrammarParser.TypeIdentifierContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#structuredType.
    def enterStructuredType(self, ctx:MyGrammarParser.StructuredTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#structuredType.
    def exitStructuredType(self, ctx:MyGrammarParser.StructuredTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unpackedStructuredType.
    def enterUnpackedStructuredType(self, ctx:MyGrammarParser.UnpackedStructuredTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unpackedStructuredType.
    def exitUnpackedStructuredType(self, ctx:MyGrammarParser.UnpackedStructuredTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#stringtype.
    def enterStringtype(self, ctx:MyGrammarParser.StringtypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#stringtype.
    def exitStringtype(self, ctx:MyGrammarParser.StringtypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#arrayType.
    def enterArrayType(self, ctx:MyGrammarParser.ArrayTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#arrayType.
    def exitArrayType(self, ctx:MyGrammarParser.ArrayTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#typeList.
    def enterTypeList(self, ctx:MyGrammarParser.TypeListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#typeList.
    def exitTypeList(self, ctx:MyGrammarParser.TypeListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#indexType.
    def enterIndexType(self, ctx:MyGrammarParser.IndexTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#indexType.
    def exitIndexType(self, ctx:MyGrammarParser.IndexTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#componentType.
    def enterComponentType(self, ctx:MyGrammarParser.ComponentTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#componentType.
    def exitComponentType(self, ctx:MyGrammarParser.ComponentTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#recordType.
    def enterRecordType(self, ctx:MyGrammarParser.RecordTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#recordType.
    def exitRecordType(self, ctx:MyGrammarParser.RecordTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#fieldList.
    def enterFieldList(self, ctx:MyGrammarParser.FieldListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#fieldList.
    def exitFieldList(self, ctx:MyGrammarParser.FieldListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#fixedPart.
    def enterFixedPart(self, ctx:MyGrammarParser.FixedPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#fixedPart.
    def exitFixedPart(self, ctx:MyGrammarParser.FixedPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#recordSection.
    def enterRecordSection(self, ctx:MyGrammarParser.RecordSectionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#recordSection.
    def exitRecordSection(self, ctx:MyGrammarParser.RecordSectionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#variantPart.
    def enterVariantPart(self, ctx:MyGrammarParser.VariantPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#variantPart.
    def exitVariantPart(self, ctx:MyGrammarParser.VariantPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#tag.
    def enterTag(self, ctx:MyGrammarParser.TagContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#tag.
    def exitTag(self, ctx:MyGrammarParser.TagContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#variant.
    def enterVariant(self, ctx:MyGrammarParser.VariantContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#variant.
    def exitVariant(self, ctx:MyGrammarParser.VariantContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#setType.
    def enterSetType(self, ctx:MyGrammarParser.SetTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#setType.
    def exitSetType(self, ctx:MyGrammarParser.SetTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#baseType.
    def enterBaseType(self, ctx:MyGrammarParser.BaseTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#baseType.
    def exitBaseType(self, ctx:MyGrammarParser.BaseTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#fileType.
    def enterFileType(self, ctx:MyGrammarParser.FileTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#fileType.
    def exitFileType(self, ctx:MyGrammarParser.FileTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#pointerType.
    def enterPointerType(self, ctx:MyGrammarParser.PointerTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#pointerType.
    def exitPointerType(self, ctx:MyGrammarParser.PointerTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#variableDeclarationPart.
    def enterVariableDeclarationPart(self, ctx:MyGrammarParser.VariableDeclarationPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#variableDeclarationPart.
    def exitVariableDeclarationPart(self, ctx:MyGrammarParser.VariableDeclarationPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#variableDeclaration.
    def enterVariableDeclaration(self, ctx:MyGrammarParser.VariableDeclarationContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#variableDeclaration.
    def exitVariableDeclaration(self, ctx:MyGrammarParser.VariableDeclarationContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#procedureAndFunctionDeclarationPart.
    def enterProcedureAndFunctionDeclarationPart(self, ctx:MyGrammarParser.ProcedureAndFunctionDeclarationPartContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#procedureAndFunctionDeclarationPart.
    def exitProcedureAndFunctionDeclarationPart(self, ctx:MyGrammarParser.ProcedureAndFunctionDeclarationPartContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#procedureOrFunctionDeclaration.
    def enterProcedureOrFunctionDeclaration(self, ctx:MyGrammarParser.ProcedureOrFunctionDeclarationContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#procedureOrFunctionDeclaration.
    def exitProcedureOrFunctionDeclaration(self, ctx:MyGrammarParser.ProcedureOrFunctionDeclarationContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#procedureDeclaration.
    def enterProcedureDeclaration(self, ctx:MyGrammarParser.ProcedureDeclarationContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#procedureDeclaration.
    def exitProcedureDeclaration(self, ctx:MyGrammarParser.ProcedureDeclarationContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#formalParameterList.
    def enterFormalParameterList(self, ctx:MyGrammarParser.FormalParameterListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#formalParameterList.
    def exitFormalParameterList(self, ctx:MyGrammarParser.FormalParameterListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#formalParameterSection.
    def enterFormalParameterSection(self, ctx:MyGrammarParser.FormalParameterSectionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#formalParameterSection.
    def exitFormalParameterSection(self, ctx:MyGrammarParser.FormalParameterSectionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#parameterGroup.
    def enterParameterGroup(self, ctx:MyGrammarParser.ParameterGroupContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#parameterGroup.
    def exitParameterGroup(self, ctx:MyGrammarParser.ParameterGroupContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#identifierList.
    def enterIdentifierList(self, ctx:MyGrammarParser.IdentifierListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#identifierList.
    def exitIdentifierList(self, ctx:MyGrammarParser.IdentifierListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#constList.
    def enterConstList(self, ctx:MyGrammarParser.ConstListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#constList.
    def exitConstList(self, ctx:MyGrammarParser.ConstListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#functionDeclaration.
    def enterFunctionDeclaration(self, ctx:MyGrammarParser.FunctionDeclarationContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#functionDeclaration.
    def exitFunctionDeclaration(self, ctx:MyGrammarParser.FunctionDeclarationContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#resultType.
    def enterResultType(self, ctx:MyGrammarParser.ResultTypeContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#resultType.
    def exitResultType(self, ctx:MyGrammarParser.ResultTypeContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#statement.
    def enterStatement(self, ctx:MyGrammarParser.StatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#statement.
    def exitStatement(self, ctx:MyGrammarParser.StatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unlabelledStatement.
    def enterUnlabelledStatement(self, ctx:MyGrammarParser.UnlabelledStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unlabelledStatement.
    def exitUnlabelledStatement(self, ctx:MyGrammarParser.UnlabelledStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#simpleStatement.
    def enterSimpleStatement(self, ctx:MyGrammarParser.SimpleStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#simpleStatement.
    def exitSimpleStatement(self, ctx:MyGrammarParser.SimpleStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#assignmentStatement.
    def enterAssignmentStatement(self, ctx:MyGrammarParser.AssignmentStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#assignmentStatement.
    def exitAssignmentStatement(self, ctx:MyGrammarParser.AssignmentStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#variable.
    def enterVariable(self, ctx:MyGrammarParser.VariableContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#variable.
    def exitVariable(self, ctx:MyGrammarParser.VariableContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#expression.
    def enterExpression(self, ctx:MyGrammarParser.ExpressionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#expression.
    def exitExpression(self, ctx:MyGrammarParser.ExpressionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#relationaloperator.
    def enterRelationaloperator(self, ctx:MyGrammarParser.RelationaloperatorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#relationaloperator.
    def exitRelationaloperator(self, ctx:MyGrammarParser.RelationaloperatorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#simpleExpression.
    def enterSimpleExpression(self, ctx:MyGrammarParser.SimpleExpressionContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#simpleExpression.
    def exitSimpleExpression(self, ctx:MyGrammarParser.SimpleExpressionContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#additiveoperator.
    def enterAdditiveoperator(self, ctx:MyGrammarParser.AdditiveoperatorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#additiveoperator.
    def exitAdditiveoperator(self, ctx:MyGrammarParser.AdditiveoperatorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#term.
    def enterTerm(self, ctx:MyGrammarParser.TermContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#term.
    def exitTerm(self, ctx:MyGrammarParser.TermContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#multiplicativeoperator.
    def enterMultiplicativeoperator(self, ctx:MyGrammarParser.MultiplicativeoperatorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#multiplicativeoperator.
    def exitMultiplicativeoperator(self, ctx:MyGrammarParser.MultiplicativeoperatorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#signedFactor.
    def enterSignedFactor(self, ctx:MyGrammarParser.SignedFactorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#signedFactor.
    def exitSignedFactor(self, ctx:MyGrammarParser.SignedFactorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#factor.
    def enterFactor(self, ctx:MyGrammarParser.FactorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#factor.
    def exitFactor(self, ctx:MyGrammarParser.FactorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#unsignedConstant.
    def enterUnsignedConstant(self, ctx:MyGrammarParser.UnsignedConstantContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#unsignedConstant.
    def exitUnsignedConstant(self, ctx:MyGrammarParser.UnsignedConstantContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#functionDesignator.
    def enterFunctionDesignator(self, ctx:MyGrammarParser.FunctionDesignatorContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#functionDesignator.
    def exitFunctionDesignator(self, ctx:MyGrammarParser.FunctionDesignatorContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#parameterList.
    def enterParameterList(self, ctx:MyGrammarParser.ParameterListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#parameterList.
    def exitParameterList(self, ctx:MyGrammarParser.ParameterListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#set_.
    def enterSet_(self, ctx:MyGrammarParser.Set_Context):
        pass

    # Exit a parse tree produced by MyGrammarParser#set_.
    def exitSet_(self, ctx:MyGrammarParser.Set_Context):
        pass


    # Enter a parse tree produced by MyGrammarParser#elementList.
    def enterElementList(self, ctx:MyGrammarParser.ElementListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#elementList.
    def exitElementList(self, ctx:MyGrammarParser.ElementListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#element.
    def enterElement(self, ctx:MyGrammarParser.ElementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#element.
    def exitElement(self, ctx:MyGrammarParser.ElementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#procedureStatement.
    def enterProcedureStatement(self, ctx:MyGrammarParser.ProcedureStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#procedureStatement.
    def exitProcedureStatement(self, ctx:MyGrammarParser.ProcedureStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#actualParameter.
    def enterActualParameter(self, ctx:MyGrammarParser.ActualParameterContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#actualParameter.
    def exitActualParameter(self, ctx:MyGrammarParser.ActualParameterContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#parameterwidth.
    def enterParameterwidth(self, ctx:MyGrammarParser.ParameterwidthContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#parameterwidth.
    def exitParameterwidth(self, ctx:MyGrammarParser.ParameterwidthContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#gotoStatement.
    def enterGotoStatement(self, ctx:MyGrammarParser.GotoStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#gotoStatement.
    def exitGotoStatement(self, ctx:MyGrammarParser.GotoStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#emptyStatement_.
    def enterEmptyStatement_(self, ctx:MyGrammarParser.EmptyStatement_Context):
        pass

    # Exit a parse tree produced by MyGrammarParser#emptyStatement_.
    def exitEmptyStatement_(self, ctx:MyGrammarParser.EmptyStatement_Context):
        pass


    # Enter a parse tree produced by MyGrammarParser#empty_.
    def enterEmpty_(self, ctx:MyGrammarParser.Empty_Context):
        pass

    # Exit a parse tree produced by MyGrammarParser#empty_.
    def exitEmpty_(self, ctx:MyGrammarParser.Empty_Context):
        pass


    # Enter a parse tree produced by MyGrammarParser#structuredStatement.
    def enterStructuredStatement(self, ctx:MyGrammarParser.StructuredStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#structuredStatement.
    def exitStructuredStatement(self, ctx:MyGrammarParser.StructuredStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#compoundStatement.
    def enterCompoundStatement(self, ctx:MyGrammarParser.CompoundStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#compoundStatement.
    def exitCompoundStatement(self, ctx:MyGrammarParser.CompoundStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#statements.
    def enterStatements(self, ctx:MyGrammarParser.StatementsContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#statements.
    def exitStatements(self, ctx:MyGrammarParser.StatementsContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#conditionalStatement.
    def enterConditionalStatement(self, ctx:MyGrammarParser.ConditionalStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#conditionalStatement.
    def exitConditionalStatement(self, ctx:MyGrammarParser.ConditionalStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#ifStatement.
    def enterIfStatement(self, ctx:MyGrammarParser.IfStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#ifStatement.
    def exitIfStatement(self, ctx:MyGrammarParser.IfStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#caseStatement.
    def enterCaseStatement(self, ctx:MyGrammarParser.CaseStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#caseStatement.
    def exitCaseStatement(self, ctx:MyGrammarParser.CaseStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#caseListElement.
    def enterCaseListElement(self, ctx:MyGrammarParser.CaseListElementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#caseListElement.
    def exitCaseListElement(self, ctx:MyGrammarParser.CaseListElementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#repetetiveStatement.
    def enterRepetetiveStatement(self, ctx:MyGrammarParser.RepetetiveStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#repetetiveStatement.
    def exitRepetetiveStatement(self, ctx:MyGrammarParser.RepetetiveStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#whileStatement.
    def enterWhileStatement(self, ctx:MyGrammarParser.WhileStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#whileStatement.
    def exitWhileStatement(self, ctx:MyGrammarParser.WhileStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#repeatStatement.
    def enterRepeatStatement(self, ctx:MyGrammarParser.RepeatStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#repeatStatement.
    def exitRepeatStatement(self, ctx:MyGrammarParser.RepeatStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#forStatement.
    def enterForStatement(self, ctx:MyGrammarParser.ForStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#forStatement.
    def exitForStatement(self, ctx:MyGrammarParser.ForStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#forList.
    def enterForList(self, ctx:MyGrammarParser.ForListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#forList.
    def exitForList(self, ctx:MyGrammarParser.ForListContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#initialValue.
    def enterInitialValue(self, ctx:MyGrammarParser.InitialValueContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#initialValue.
    def exitInitialValue(self, ctx:MyGrammarParser.InitialValueContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#finalValue.
    def enterFinalValue(self, ctx:MyGrammarParser.FinalValueContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#finalValue.
    def exitFinalValue(self, ctx:MyGrammarParser.FinalValueContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#withStatement.
    def enterWithStatement(self, ctx:MyGrammarParser.WithStatementContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#withStatement.
    def exitWithStatement(self, ctx:MyGrammarParser.WithStatementContext):
        pass


    # Enter a parse tree produced by MyGrammarParser#recordVariableList.
    def enterRecordVariableList(self, ctx:MyGrammarParser.RecordVariableListContext):
        pass

    # Exit a parse tree produced by MyGrammarParser#recordVariableList.
    def exitRecordVariableList(self, ctx:MyGrammarParser.RecordVariableListContext):
        pass



del MyGrammarParser