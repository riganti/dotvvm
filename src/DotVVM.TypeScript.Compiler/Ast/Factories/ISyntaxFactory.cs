using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast.Factories
{
    public interface ISyntaxFactory
    {
        IAssignmentSyntax CreateAssignment(IReferenceSyntax reference, IExpressionSyntax expression, ISyntaxNode parent);
        IBinaryOperationSyntax CreateBinaryOperation(IExpressionSyntax left, BinaryOperator @operator,
            IExpressionSyntax right, ISyntaxNode parent);
        IBlockSyntax CreateBlock(IList<IStatementSyntax> statements, ISyntaxNode parent);
        IClassDeclarationSyntax CreateClassDeclaration(IIdentifierSyntax identifier,
            IList<IMemberDeclarationSyntax> members, IList<IIdentifierSyntax> baseClasses, ISyntaxNode parent);
        IConditionalExpressionSyntax CreateConditionalExpression(IExpressionSyntax condition,
            IExpressionSyntax whenTrue, IExpressionSyntax whenFalse, ISyntaxNode parent);
        IDoWhileStatementSyntax CreateDoWhileStatement(IExpressionSyntax condition, IStatementSyntax body,
            ISyntaxNode parent);
        IForStatementSyntax CreateForStatement(IStatementSyntax before, IExpressionSyntax condition,
            IStatementSyntax after, IStatementSyntax body, ISyntaxNode parent);

        IIdentifierSyntax CreateIdentifier(string value, ISyntaxNode parent);
        IIfStatementSyntax CreateIfStatement(IExpressionSyntax condition, IStatementSyntax whenTrue,
            IStatementSyntax whenFalse, ISyntaxNode parent);
        IIncrementOrDecrementSyntax CreateIncrementOrDecrement(IExpressionSyntax target, bool isPostfix,
            bool isIncrement, ISyntaxNode parent);
        ILiteralExpressionSyntax CreateLiteralExpression(string value, ISyntaxNode parent);
        ILocalVariableReferenceSyntax CreateLocalVariableReference(IIdentifierSyntax identififer, ISyntaxNode parent);
        ILocalVariableDeclarationSyntax CreateLocalVariableDeclaration(IList<IVariableDeclaratorSyntax> declarators,
            ISyntaxNode parent);

        IMethodDeclarationSyntax CreateMethodDeclaration(AccessModifier modifier, IIdentifierSyntax identifier,
            ISyntaxNode parent, IBlockSyntax body, IList<IParameterSyntax> parameters);
        IMethodCallSyntax CreateMethodCall(IIdentifierSyntax name, ImmutableList<IExpressionSyntax> parameters,
            ISyntaxNode parent);
        INamespaceDeclarationSyntax CreateNamespaceDeclaration(IIdentifierSyntax identifier,
            IList<IClassDeclarationSyntax> classes, ISyntaxNode parent);
        IParameterSyntax CreateParameter(IIdentifierSyntax identifier, ITypeSyntax type, ISyntaxNode parent);
        IParenthesizedExpressionSyntax CreateParenthesizedExpression(IExpressionSyntax expresion, ISyntaxNode parent);
        IPropertyDeclarationSyntax CreatePropertyDeclarationSyntax(AccessModifier modifier,
            IIdentifierSyntax identifier, ITypeSyntax type, ISyntaxNode parent);
        IPropertyReferenceSyntax CreatePropertyReferenceSyntax(IIdentifierSyntax identifier, ISyntaxNode parent);
        IReturnStatementSyntax CreateReturnStatement(IExpressionSyntax expression, ISyntaxNode parent);
        IUnaryOperationSyntax CreateUnaryOperation(IExpressionSyntax operand, UnaryOperator @operator,
            ISyntaxNode parent);
        IVariableDeclaratorSyntax CreateVariableDeclarator(IExpressionSyntax expression, IIdentifierSyntax identifier,
            ISyntaxNode parent);

        IWhileStatementSyntax CreateWhileStatement(IExpressionSyntax condition, IStatementSyntax body,
            ISyntaxNode parent);

        ITypeSyntax CreateType(ITypeSymbol type, ISyntaxNode parent);
    }
}
