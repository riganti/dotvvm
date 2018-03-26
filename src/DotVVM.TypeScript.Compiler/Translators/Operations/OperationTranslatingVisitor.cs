using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis.Operations;

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
                if(syntaxNode is TsStatementSyntax statementSyntax)
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
            var identifier = operation.Target.Accept(this, parent) as TsIdentifierSyntax;
            var expression = operation.Value.Accept(this, parent) as TsExpressionSyntax;
            var assignment = new TsAssignmentSyntax(parent, identifier, expression);
            return assignment;
        }

        public override TsSyntaxNode VisitPropertyReference(IPropertyReferenceOperation operation, TsSyntaxNode parent)
        {
            return new TsIdentifierSyntax(operation.Property.Name, parent);
        }

    }
}
