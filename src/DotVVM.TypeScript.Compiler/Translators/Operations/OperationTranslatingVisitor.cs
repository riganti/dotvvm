using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis.Operations;
using DotVVM.TypeScript.Compiler.Symbols;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Operations
{
    class OperationTranslatingVisitor : OperationVisitor<TsSyntaxNode, TsSyntaxNode>
    {
        public override TsSyntaxNode VisitBlock(IBlockOperation blockOperation, TsSyntaxNode parent)
        {
            var blockSyntax = new TsBlockSyntax(parent, new List<TsStatementSyntax>());
            foreach (var operation in blockOperation.Operations)
            {
                var syntaxNode = operation.Accept(this, blockSyntax);
                if (syntaxNode is TsStatementSyntax statementSyntax)
                {
                    blockSyntax.AddStatement(statementSyntax);
                }
            }
            return blockSyntax;
        }

        public override TsSyntaxNode VisitExpressionStatement(IExpressionStatementOperation operation, TsSyntaxNode argument)
        {
            return operation.Operation.Accept(this, argument);
        }

        public override TsSyntaxNode VisitLiteral(ILiteralOperation operation, TsSyntaxNode argument)
        {
            string value = "";
            if (operation.ConstantValue.HasValue)
            {
                value = operation.ConstantValue.ToString();
            }
            return new TsLiteralExpressionSyntax(argument, value);
        }
        
        public override TsSyntaxNode VisitSimpleAssignment(ISimpleAssignmentOperation operation, TsSyntaxNode parent)
        {
            var identifier = operation.Target.Accept(this, parent) as TsIdentifierReferenceSyntax;
            var expression = operation.Value.Accept(this, parent) as TsExpressionSyntax;
            var assignment = new TsAssignmentSyntax(parent, identifier, expression);
            return assignment;
        }

        public override TsSyntaxNode VisitUnaryOperator(IUnaryOperation operation, TsSyntaxNode argument)
        {
            var operand = operation.Operand.Accept(this, argument) as TsExpressionSyntax;
            var unaryOperator = operation.OperatorKind.ToTsUnaryOperator();
            return new TsUnaryOperationSyntax(argument, operand, unaryOperator);
        }

        public override TsSyntaxNode VisitBinaryOperator(IBinaryOperation operation, TsSyntaxNode parent)
        {
            var left = operation.LeftOperand.Accept(this, parent) as TsExpressionSyntax;
            var binaryOperator = operation.OperatorKind.ToTsBinaryOperator();
            var right = operation.RightOperand.Accept(this, parent) as TsExpressionSyntax;
            return new TsBinaryOperationSyntax(parent, left, right, binaryOperator);
        }

        public override TsSyntaxNode VisitLocalReference(ILocalReferenceOperation operation, TsSyntaxNode argument)
        {
            return new TsIdentifierSyntax(operation.Local.Name, argument);
        }

        public override TsSyntaxNode VisitPropertyReference(IPropertyReferenceOperation operation, TsSyntaxNode parent)
        {
            var identifier = new TsIdentifierSyntax(operation.Property.Name, parent);
            return new TsIdentifierReferenceSyntax(parent, identifier);
        }

    }
}
