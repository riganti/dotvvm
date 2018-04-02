using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface ITsNodeVisitor
    {
        void VisitAssignmentStatement(TsAssignmentSyntax assignment);
        void VisitBinaryOperation(TsBinaryOperationSyntax binaryOperation);
        void VisitBlockStatement(TsBlockSyntax block);
        void VisitClassDeclaration(TsClassDeclarationSyntax classDeclaration);
        void VisitConditionalExpression(TsConditionalExpressionSyntax conditionalExpression);
        void VisitDoWhileStatement(TsDoWhileStatementSyntax doWhileStatement);
        void VisitForStatement(TsForStatementSyntax forStatement);
        void VisitIdentifierReference(TsIdentifierReferenceSyntax reference);
        void VisitIdentifier(TsIdentifierSyntax identifier);
        void VisitIfStatement(TsIfStatementSyntax ifStatement);
        void VisitIncrementOrDecrementOperation(TsIncrementOrDecrementSyntax incrementOrDecrement);
        void VisitLiteral(TsLiteralExpressionSyntax literal);
        void VisitLocalVariableDeclaration(TsLocalVariableDeclarationSyntax declaration);
        void VisitMethodDeclaration(TsMethodDeclarationSyntax methodDeclaration);
        void VisitNamespaceDeclaration(TsNamespaceDeclarationSyntax namespaceDeclaration);
        void VisitParameter(TsParameterSyntax parameter);
        void VisitParenthesizedExpression(TsParenthesizedExpressionSyntax expression);
        void VisitPropertyDeclaration(TsPropertyDeclarationSyntax propertyDeclaration);
        void VisitReturnStatement(TsReturnStatementSyntax returnStatement);
        void VisitType(TsTypeSyntax typeSyntax);
        void VisitUnaryOperation(TsUnaryOperationSyntax unaryOperation);
        void VisitVariableDeclarator(TsVariableDeclaratorSyntax variableDeclarator);
        void VisitWhileStatement(TsWhileStatementSyntax whileStatement);
    }

    class TsNodeVisitor : ITsNodeVisitor
    {
        public void VisitAssignmentStatement(TsAssignmentSyntax assignment)
        {
            DefaultVisit(assignment);
        }

        public void VisitBinaryOperation(TsBinaryOperationSyntax binaryOperation)
        {
            DefaultVisit(binaryOperation);
        }

        public void VisitBlockStatement(TsBlockSyntax block)
        {
            DefaultVisit(block);
        }

        public void VisitClassDeclaration(TsClassDeclarationSyntax classDeclaration)
        {
            DefaultVisit(classDeclaration);
        }

        public void VisitConditionalExpression(TsConditionalExpressionSyntax conditionalExpression)
        {
            DefaultVisit(conditionalExpression);
        }

        public void VisitDoWhileStatement(TsDoWhileStatementSyntax doWhileStatement)
        {
            DefaultVisit(doWhileStatement);
        }

        public void VisitForStatement(TsForStatementSyntax forStatement)
        {
            DefaultVisit(forStatement);
        }

        public void VisitIdentifierReference(TsIdentifierReferenceSyntax reference)
        {
            DefaultVisit(reference);
        }

        public void VisitIdentifier(TsIdentifierSyntax identifier)
        {
            DefaultVisit(identifier);
        }

        public void VisitIfStatement(TsIfStatementSyntax ifStatement)
        {
            DefaultVisit(ifStatement);
        }

        public void VisitIncrementOrDecrementOperation(TsIncrementOrDecrementSyntax incrementOrDecrement)
        {
            DefaultVisit(incrementOrDecrement);
        }

        public void VisitLiteral(TsLiteralExpressionSyntax literal)
        {
            DefaultVisit(literal);
        }

        public void VisitLocalVariableDeclaration(TsLocalVariableDeclarationSyntax declaration)
        {
            DefaultVisit(declaration);
        }

        public void VisitMethodDeclaration(TsMethodDeclarationSyntax methodDeclaration)
        {
            DefaultVisit(methodDeclaration);
        }

        public void VisitNamespaceDeclaration(TsNamespaceDeclarationSyntax namespaceDeclaration)
        {
            DefaultVisit(namespaceDeclaration);
        }

        public void VisitParameter(TsParameterSyntax parameter)
        {
            DefaultVisit(parameter);
        }

        public void VisitParenthesizedExpression(TsParenthesizedExpressionSyntax expression)
        {
            DefaultVisit(expression);
        }

        public void VisitPropertyDeclaration(TsPropertyDeclarationSyntax propertyDeclaration)
        {
            DefaultVisit(propertyDeclaration);
        }

        public void VisitReturnStatement(TsReturnStatementSyntax returnStatement)
        {
            DefaultVisit(returnStatement);
        }

        public void VisitType(TsTypeSyntax typeSyntax)
        {
            DefaultVisit(typeSyntax);
        }

        public void VisitUnaryOperation(TsUnaryOperationSyntax unaryOperation)
        {
            DefaultVisit(unaryOperation);
        }

        public void VisitVariableDeclarator(TsVariableDeclaratorSyntax variableDeclarator)
        {
            DefaultVisit(variableDeclarator);
        }

        public void VisitWhileStatement(TsWhileStatementSyntax whileStatement)
        {
            DefaultVisit(whileStatement);
        }

        protected void DefaultVisit(TsSyntaxNode node)
        {
        }
    }
}
