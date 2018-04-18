using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast.Factories
{
    public class TypeScriptSyntaxFactory : ISyntaxFactory
    {
        public IAssignmentSyntax CreateAssignment(IReferenceSyntax reference, IExpressionSyntax expression, ISyntaxNode parent)
        {
            return new TsAssignmentSyntax(parent, reference, expression);
        }

        public IBinaryOperationSyntax CreateBinaryOperation(IExpressionSyntax left, BinaryOperator @operator, IExpressionSyntax right,
            ISyntaxNode parent)
        {
            return new TsBinaryOperationSyntax(parent, left, @operator, right);
        }

        public IBlockSyntax CreateBlock(IList<IStatementSyntax> statements, ISyntaxNode parent)
        {
            return new TsBlockSyntax(parent, statements);
        }

        public IClassDeclarationSyntax CreateClassDeclaration(IIdentifierSyntax identifier, IList<IMemberDeclarationSyntax> members, IList<IIdentifierSyntax> baseClasses,
            ISyntaxNode parent)
        {
            return new TsClassDeclarationSyntax(parent, identifier, members, baseClasses);
        }

        public IConditionalExpressionSyntax CreateConditionalExpression(IExpressionSyntax condition, IExpressionSyntax whenTrue,
            IExpressionSyntax whenFalse, ISyntaxNode parent)
        {
            return new TsConditionalExpressionSyntax(parent, condition, whenTrue, whenFalse);
        }

        public IDoWhileStatementSyntax CreateDoWhileStatement(IExpressionSyntax condition, IStatementSyntax body, ISyntaxNode parent)
        {
            return new TsDoWhileStatementSyntax(parent, condition, body);
        }

        public IForStatementSyntax CreateForStatement(IStatementSyntax before, IExpressionSyntax condition, IStatementSyntax after,
            IStatementSyntax body, ISyntaxNode parent)
        {
            return new TsForStatementSyntax(parent, before, condition, after, body);
        }

        public IIdentifierSyntax CreateIdentifier(string value, ISyntaxNode parent)
        {
            return new TsIdentifierSyntax(value, parent);
        }

        public IIfStatementSyntax CreateIfStatement(IExpressionSyntax condition, IStatementSyntax whenTrue,
            IStatementSyntax whenFalse, ISyntaxNode parent)
        {
            return new TsIfStatementSyntax(parent, condition, whenTrue, whenFalse);
        }

        public IIncrementOrDecrementSyntax CreateIncrementOrDecrement(IExpressionSyntax target, bool isPostfix, bool isIncrement,
            ISyntaxNode parent)
        {
            return new TsIncrementOrDecrementSyntax(parent, target, isPostfix, isIncrement);
        }

        public IInstanceReferenceSyntax CreateInstanceReference(ISyntaxNode parent)
        {
            return new TsInstanceReferenceSyntax(parent);
        }

        public ILiteralExpressionSyntax CreateLiteralExpression(string value, ISyntaxNode parent)
        {
            return new TsLiteralExpressionSyntax(parent, value);
        }

        public ILocalVariableReferenceSyntax CreateLocalVariableReference(IIdentifierSyntax identififer, ISyntaxNode parent)
        {
            return new TsLocalVariableReferenceSyntax(parent, identififer);
        }

        public ILocalVariableDeclarationSyntax CreateLocalVariableDeclaration(IList<IVariableDeclaratorSyntax> declarators, ISyntaxNode parent)
        {
            return new TsLocalVariableDeclarationSyntax(parent, declarators);
        }

        public IRawSyntaxNode CreateRawSyntaxNode(string value, ISyntaxNode parent)
        {
            return new TsRawSyntaxNode(parent, value);
        }

        public IMethodDeclarationSyntax CreateMethodDeclaration(AccessModifier modifier, IIdentifierSyntax identifier,
            ISyntaxNode parent, IBlockSyntax body, IList<IParameterSyntax> parameters)
        {
            return new TsMethodDeclarationSyntax(modifier, identifier, parent, body, parameters);
        }

        public IMethodCallSyntax CreateMethodCall(IReferenceSyntax @object, IIdentifierSyntax name, ImmutableList<IExpressionSyntax> parameters, ISyntaxNode parent)
        {
            return new TsMethodCallSyntax( parent, name, parameters, @object);
        }

        public INamespaceDeclarationSyntax CreateNamespaceDeclaration(IIdentifierSyntax identifier, IList<IClassDeclarationSyntax> classes, ISyntaxNode parent)
        {
            return new TsNamespaceDeclarationSyntax(parent, identifier, classes);
        }

        public IParameterSyntax CreateParameter(IIdentifierSyntax identifier, ITypeSyntax type, ISyntaxNode parent)
        {
            return new TsParameterSyntax(parent, identifier, type);
        }

        public IParenthesizedExpressionSyntax CreateParenthesizedExpression(IExpressionSyntax expresion,
            ISyntaxNode parent)
        {
            return  new TsParenthesizedExpressionSyntax(parent, expresion);
        }

        public IPropertyDeclarationSyntax CreatePropertyDeclarationSyntax(AccessModifier modifier,
            IIdentifierSyntax identifier, ITypeSyntax type, ISyntaxNode parent)
        {
            return new TsPropertyDeclarationSyntax(modifier, identifier, parent, type);
        }

        public IPropertyReferenceSyntax CreatePropertyReferenceSyntax(IReferenceSyntax instance,
            IIdentifierSyntax identifier, ISyntaxNode parent, ITypeSymbol type)
        {
            return new TsPropertyReferenceSyntax(parent, identifier, instance, type);
        }

        public IReturnStatementSyntax CreateReturnStatement(IExpressionSyntax expression, ISyntaxNode parent)
        {
            return new TsReturnStatementSyntax(parent, expression);
        }

        public IUnaryOperationSyntax CreateUnaryOperation(IExpressionSyntax operand, UnaryOperator @operator, ISyntaxNode parent)
        {
            return new TsUnaryOperationSyntax(parent, operand, @operator);
        }

        public IVariableDeclaratorSyntax CreateVariableDeclarator(IExpressionSyntax expression, IIdentifierSyntax identifier,
            ISyntaxNode parent)
        {
            return new TsVariableDeclaratorSyntax(parent, expression, identifier);
        }

        public IWhileStatementSyntax CreateWhileStatement(IExpressionSyntax condition, IStatementSyntax body, ISyntaxNode parent)
        {
            return new TsWhileStatementSyntax(parent, condition, body);
        }

        public ITypeSyntax CreateType(ITypeSymbol type, ISyntaxNode parent)
        {
            return new TsTypeSyntax(type, parent);
        }

        public IArrayElementReferenceSyntax CreateArrayElementReference(IReferenceSyntax arrayReference,
            IExpressionSyntax itemReference, ISyntaxNode parent)
        {
            return new TsArrayElementReferenceSyntax(parent, arrayReference, itemReference);
        }

        public IObjectCreationExpressionSyntax CreateObjectCreationExpression(ITypeSyntax type, IList<IExpressionSyntax> arguments, ISyntaxNode parent)
        {
            return new TsObjectCreationExpressionSyntax(parent, arguments, type);
        }
    }
}
