namespace DotVVM.TypeScript.Compiler.Ast.Visitors
{
    public interface INodeVisitor
    {
        void VisitAssignmentStatement(IAssignmentSyntax assignment);
        void VisitBinaryOperation(IBinaryOperationSyntax binaryOperation);
        void VisitBlockStatement(IBlockSyntax block);
        void VisitClassDeclaration(IClassDeclarationSyntax classDeclaration);
        void VisitConditionalExpression(IConditionalExpressionSyntax conditionalExpression);
        void VisitDoWhileStatement(IDoWhileStatementSyntax doWhileStatement);
        void VisitForStatement(IForStatementSyntax forStatement);
        void VisitIdentifierReference(ILocalVariableReferenceSyntax reference);
        void VisitIdentifier(IIdentifierSyntax identifier);
        void VisitIfStatement(IIfStatementSyntax ifStatement);
        void VisitIncrementOrDecrementOperation(IIncrementOrDecrementSyntax incrementOrDecrement);
        void VisitLiteral(ILiteralExpressionSyntax literal);
        void VisitLocalVariableDeclaration(ILocalVariableDeclarationSyntax declaration);
        void VisitMethodDeclaration(IMethodDeclarationSyntax methodDeclaration);
        void VisitNamespaceDeclaration(INamespaceDeclarationSyntax namespaceDeclaration);
        void VisitParameter(IParameterSyntax parameter);
        void VisitParenthesizedExpression(IParenthesizedExpressionSyntax expression);
        void VisitPropertyDeclaration(IPropertyDeclarationSyntax propertyDeclaration);
        void VisitReturnStatement(IReturnStatementSyntax returnStatement);
        void VisitType(ITypeSyntax typeSyntax);
        void VisitUnaryOperation(IUnaryOperationSyntax unaryOperation);
        void VisitVariableDeclarator(IVariableDeclaratorSyntax variableDeclarator);
        void VisitWhileStatement(IWhileStatementSyntax whileStatement);
        void VisitPropertyReference(IPropertyReferenceSyntax tsPropertyReferenceSyntax);
        void VisitMethodCall(IMethodCallSyntax methodCall);
    }

    class NodeVisitor : INodeVisitor
    {
        public void VisitAssignmentStatement(IAssignmentSyntax assignment)
        {
            DefaultVisit(assignment);
        }

        public void VisitBinaryOperation(IBinaryOperationSyntax binaryOperation)
        {
            DefaultVisit(binaryOperation);
        }

        public void VisitBlockStatement(IBlockSyntax block)
        {
            DefaultVisit(block);
        }

        public void VisitClassDeclaration(IClassDeclarationSyntax classDeclaration)
        {
            DefaultVisit(classDeclaration);
        }

        public void VisitConditionalExpression(IConditionalExpressionSyntax conditionalExpression)
        {
            DefaultVisit(conditionalExpression);
        }

        public void VisitDoWhileStatement(IDoWhileStatementSyntax doWhileStatement)
        {
            DefaultVisit(doWhileStatement);
        }

        public void VisitForStatement(IForStatementSyntax forStatement)
        {
            DefaultVisit(forStatement);
        }

        public void VisitIdentifierReference(ILocalVariableReferenceSyntax reference)
        {
            DefaultVisit(reference);
        }

        public void VisitIdentifier(IIdentifierSyntax identifier)
        {
            DefaultVisit(identifier);
        }

        public void VisitIfStatement(IIfStatementSyntax ifStatement)
        {
            DefaultVisit(ifStatement);
        }

        public void VisitIncrementOrDecrementOperation(IIncrementOrDecrementSyntax incrementOrDecrement)
        {
            DefaultVisit(incrementOrDecrement);
        }

        public void VisitLiteral(ILiteralExpressionSyntax literal)
        {
            DefaultVisit(literal);
        }

        public void VisitLocalVariableDeclaration(ILocalVariableDeclarationSyntax declaration)
        {
            DefaultVisit(declaration);
        }

        public void VisitMethodDeclaration(IMethodDeclarationSyntax methodDeclaration)
        {
            DefaultVisit(methodDeclaration);
        }

        public void VisitNamespaceDeclaration(INamespaceDeclarationSyntax namespaceDeclaration)
        {
            DefaultVisit(namespaceDeclaration);
        }

        public void VisitParameter(IParameterSyntax parameter)
        {
            DefaultVisit(parameter);
        }

        public void VisitParenthesizedExpression(IParenthesizedExpressionSyntax expression)
        {
            DefaultVisit(expression);
        }

        public void VisitPropertyDeclaration(IPropertyDeclarationSyntax propertyDeclaration)
        {
            DefaultVisit(propertyDeclaration);
        }

        public void VisitReturnStatement(IReturnStatementSyntax returnStatement)
        {
            DefaultVisit(returnStatement);
        }

        public void VisitType(ITypeSyntax typeSyntax)
        {
            DefaultVisit(typeSyntax);
        }

        public void VisitUnaryOperation(IUnaryOperationSyntax unaryOperation)
        {
            DefaultVisit(unaryOperation);
        }

        public void VisitVariableDeclarator(IVariableDeclaratorSyntax variableDeclarator)
        {
            DefaultVisit(variableDeclarator);
        }

        public void VisitWhileStatement(IWhileStatementSyntax whileStatement)
        {
            DefaultVisit(whileStatement);
        }

        public void VisitPropertyReference(IPropertyReferenceSyntax tsPropertyReferenceSyntax)
        {
            DefaultVisit(tsPropertyReferenceSyntax);
        }

        public void VisitMethodCall(IMethodCallSyntax methodCall)
        {
            DefaultVisit(methodCall);
        }

        protected void DefaultVisit(ISyntaxNode node)
        {
        }
    }
}
