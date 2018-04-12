using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using Microsoft.CodeAnalysis.Operations;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Operations
{
    class OperationTranslatingVisitor : OperationVisitor<ISyntaxNode, ISyntaxNode>
    {
        private readonly ILogger _logger;

        public OperationTranslatingVisitor(ILogger logger)
        {
            _logger = logger;
        }

        public override ISyntaxNode VisitBlock(IBlockOperation blockOperation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating block operation.");
            var blockSyntax = new TsBlockSyntax(parent, new List<IStatementSyntax>());
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

        public override ISyntaxNode VisitExpressionStatement(IExpressionStatementOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating expression operation.");
            return operation.Operation.Accept(this, argument);
        }

        public override ISyntaxNode VisitVariableDeclaration(IVariableDeclarationOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating variable declaration operation.");
            var declarators = new List<IVariableDeclaratorSyntax>();
            foreach (var declarator in operation.Declarators)
            {
                var syntax = declarator.Accept(this, argument);
                if (syntax is TsVariableDeclaratorSyntax declaratorSyntax)
                {
                    declarators.Add(declaratorSyntax);
                }
            }
            return new TsLocalVariableDeclarationSyntax(argument, declarators);
        }

        public override ISyntaxNode VisitVariableDeclarator(IVariableDeclaratorOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating variable declarator operation.");
            var identifier = new TsIdentifierSyntax(operation.Symbol.Name, argument);
            var expression = operation.Initializer?.Accept(this, argument);
            return new TsVariableDeclaratorSyntax(argument, expression as TsExpressionSyntax, identifier);
        }

        public override ISyntaxNode VisitVariableInitializer(IVariableInitializerOperation operation, ISyntaxNode argument)
        {
            return operation.Value?.Accept(this, argument);
        }

        public override ISyntaxNode VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, ISyntaxNode argument)
        {
            return operation.Declarations.Single().Accept(this, argument);
        }

        public override ISyntaxNode VisitReturn(IReturnOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating return operation.");
            var expression = operation.ReturnedValue?.Accept(this, argument) as TsExpressionSyntax;
            return new TsReturnStatementSyntax(argument, expression);
        }

        public override ISyntaxNode VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating increment or decrement operation.");
            var target = operation.Target.Accept(this, argument) as TsExpressionSyntax;
            var isIncrement = operation.Kind == OperationKind.Increment;
            return new TsIncrementOrDecrementSyntax(argument,
                target,
                operation.IsPostfix,
                isIncrement);
        }

        public override ISyntaxNode VisitForLoop(IForLoopOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating for loop operation.");
            var beforeStatement = operation.Before.FirstOrDefault().Accept(this, argument) as TsStatementSyntax;
            var condition = operation.Condition.Accept(this, argument) as TsExpressionSyntax;
            var afterStatement = operation.AtLoopBottom.First().Accept(this, argument) as TsStatementSyntax;
            var body = operation.Body.Accept(this, argument) as TsStatementSyntax;
            return new TsForStatementSyntax(argument,
                beforeStatement,
                condition,
                afterStatement,
                body);
        }

        public override ISyntaxNode VisitWhileLoop(IWhileLoopOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating while loop operation.");
            var condition = operation.Condition.Accept(this, argument) as TsExpressionSyntax;
            var body = operation.Body.Accept(this, argument) as TsStatementSyntax;
            if(operation.ConditionIsTop)
                return new TsWhileStatementSyntax(argument, condition, body);
            else
                return new TsDoWhileStatementSyntax(argument, condition, body);
        }

        public override ISyntaxNode VisitConditional(IConditionalOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating conditional operation.");
            var expression = operation.Condition.Accept(this, argument) as TsExpressionSyntax;
            var trueStatement = operation.WhenTrue.Accept(this, argument);
            var falseStatement = operation.WhenFalse?.Accept(this, argument);

            if (operation.Type == null)
            {
                return new TsIfStatementSyntax(argument,
                    expression,
                    trueStatement as TsStatementSyntax,
                    falseStatement as TsStatementSyntax);
            }
            else
            {
                return new TsConditionalExpressionSyntax(argument,
                    expression,
                    trueStatement as TsExpressionSyntax,
                    falseStatement as TsExpressionSyntax);
            }
        }

        public override ISyntaxNode VisitLiteral(ILiteralOperation operation, ISyntaxNode argument)
        {
            string value = "";
            if (operation.ConstantValue.HasValue)
            {
                value = operation.ConstantValue.ToString();
            }
            return new TsLiteralExpressionSyntax(argument, value);
        }
        
        public override ISyntaxNode VisitSimpleAssignment(ISimpleAssignmentOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating simple assignment operation.");
            var identifier = operation.Target.Accept(this, parent) as IReferenceSyntax;
            var expression = operation.Value.Accept(this, parent) as IExpressionSyntax;
            if (operation.Type.IsIntegerType())
            {
                var methodIdentifier = new TsIdentifierSyntax("Math.floor", parent);
                var parameters = new List<IExpressionSyntax> {expression};
                expression = new TsMethodCallSyntax(parent,methodIdentifier , parameters.ToImmutableList());
            }
            var assignment = new TsAssignmentSyntax(parent, identifier, expression);
            return assignment;
        }

        public override ISyntaxNode VisitUnaryOperator(IUnaryOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating unary operation.");
            var operand = operation.Operand.Accept(this, argument) as TsExpressionSyntax;
            var unaryOperator = operation.OperatorKind.ToTsUnaryOperator(); 
            return new TsUnaryOperationSyntax(argument, operand, unaryOperator);
        }

        public override ISyntaxNode VisitBinaryOperator(IBinaryOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating binary operation.");
            var left = operation.LeftOperand.Accept(this, parent) as TsExpressionSyntax;
            if (operation.LeftOperand is IBinaryOperation)
            {
                left = new TsParenthesizedExpressionSyntax(parent, left);
            }
            var binaryOperator = operation.OperatorKind.ToTsBinaryOperator();
            var right = operation.RightOperand.Accept(this, parent) as TsExpressionSyntax;
            if (operation.RightOperand is IBinaryOperation)
            {
                right = new TsParenthesizedExpressionSyntax(parent, right);
            }
            return new TsBinaryOperationSyntax(parent, left, right, binaryOperator);
        }

        public override ISyntaxNode VisitLocalReference(ILocalReferenceOperation operation, ISyntaxNode argument)
        {
            _logger.LogDebug("Operations", "Translating local reference operation.");
            var identifier = new TsIdentifierSyntax(operation.Local.Name, argument);
            return new TsLocalVariableReferenceSyntax(argument, identifier);
        }

        public override ISyntaxNode VisitPropertyReference(IPropertyReferenceOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating property reference operation.");
            var identifier = new TsIdentifierSyntax($"this.{operation.Property.Name}", parent);
            return new TsPropertyReferenceSyntax(parent, identifier);
        }

    }
}

