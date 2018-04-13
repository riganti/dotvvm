using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators.Operations
{
    internal class OperationTranslatingVisitor : OperationVisitor<ISyntaxNode, ISyntaxNode>
    {
        private readonly ILogger _logger;
        private readonly ISyntaxFactory _factory;
        private readonly IBuiltinMethodTranslatorRegistry _methodTranslatorRegistry;

        public OperationTranslatingVisitor(ILogger logger, ISyntaxFactory factory, IBuiltinMethodTranslatorRegistry methodTranslatorRegistry)
        {
            _logger = logger;
            _factory = factory;
            _methodTranslatorRegistry = methodTranslatorRegistry;
        }

        public override ISyntaxNode VisitBlock(IBlockOperation blockOperation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating block operation.");
            var blockSyntax = _factory.CreateBlock(new List<IStatementSyntax>(), parent);
            foreach (var operation in blockOperation.Operations)
            {
                var syntaxNode = operation.Accept(this, blockSyntax);
                if (syntaxNode is IStatementSyntax statementSyntax) blockSyntax.AddStatement(statementSyntax);
            }

            return blockSyntax;
        }

        public override ISyntaxNode VisitExpressionStatement(IExpressionStatementOperation operation,
            ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating expression operation.");
            return operation.Operation.Accept(this, parent);
        }

        public override ISyntaxNode VisitVariableDeclaration(IVariableDeclarationOperation operation,
            ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating variable declaration operation.");
            var declarators = new List<IVariableDeclaratorSyntax>();
            foreach (var declarator in operation.Declarators)
            {
                var syntax = declarator.Accept(this, parent);
                if (syntax is IVariableDeclaratorSyntax declaratorSyntax) declarators.Add(declaratorSyntax);
            }

            return _factory.CreateLocalVariableDeclaration(declarators, parent);
        }

        public override ISyntaxNode VisitVariableDeclarator(IVariableDeclaratorOperation operation,
            ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating variable declarator operation.");
            var identifier = _factory.CreateIdentifier(operation.Symbol.Name, parent);
            var expression = operation.Initializer?.Accept(this, parent) as IExpressionSyntax;
            return _factory.CreateVariableDeclarator(expression, identifier, parent);
        }

        public override ISyntaxNode VisitVariableInitializer(IVariableInitializerOperation operation,
            ISyntaxNode parent)
        {
            return operation.Value?.Accept(this, parent);
        }

        public override ISyntaxNode VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation,
            ISyntaxNode parent)
        {
            return operation.Declarations.Single().Accept(this, parent);
        }

        public override ISyntaxNode VisitReturn(IReturnOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating return operation.");
            var expression = operation.ReturnedValue?.Accept(this, parent) as IExpressionSyntax;
            return _factory.CreateReturnStatement(expression, parent);
        }

        public override ISyntaxNode VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation,
            ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating increment or decrement operation.");
            var target = operation.Target.Accept(this, parent) as IExpressionSyntax;
            if (target is IPropertyReferenceSyntax)
            {
                var @operator = operation.Kind == OperationKind.Increment
                    ? BinaryOperator.Add
                    : BinaryOperator.Subtract;
                var literal = _factory.CreateLiteralExpression("1", parent);
                var binaryOperation = _factory.CreateBinaryOperation(target, @operator, literal, parent);
                return _factory.CreateAssignment(target as IReferenceSyntax, binaryOperation, parent);
            }
            var isIncrement = operation.Kind == OperationKind.Increment;
            return _factory.CreateIncrementOrDecrement(target, operation.IsPostfix, isIncrement, parent);
        }

        public override ISyntaxNode VisitForLoop(IForLoopOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating for loop operation.");
            var beforeStatement = operation.Before.FirstOrDefault().Accept(this, parent) as IStatementSyntax;
            var condition = operation.Condition.Accept(this, parent) as IExpressionSyntax;
            var afterStatement = operation.AtLoopBottom.First().Accept(this, parent) as IStatementSyntax;
            var body = operation.Body.Accept(this, parent) as TsStatementSyntax;
            return _factory.CreateForStatement(beforeStatement, condition, afterStatement, body, parent);
        }

        public override ISyntaxNode VisitWhileLoop(IWhileLoopOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating while loop operation.");
            var condition = operation.Condition.Accept(this, parent) as IExpressionSyntax;
            var body = operation.Body.Accept(this, parent) as IStatementSyntax;
            if (operation.ConditionIsTop)
                return _factory.CreateWhileStatement(condition, body, parent);
            else
                return _factory.CreateDoWhileStatement(condition, body, parent);
        }

        public override ISyntaxNode VisitConditional(IConditionalOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating conditional operation.");
            var expression = operation.Condition.Accept(this, parent) as IExpressionSyntax;
            var trueStatement = operation.WhenTrue.Accept(this, parent);
            var falseStatement = operation.WhenFalse?.Accept(this, parent);

            if (operation.Type == null)
                return _factory.CreateIfStatement(expression, trueStatement as IStatementSyntax,
                    falseStatement as IStatementSyntax, parent);
            else
                return _factory.CreateConditionalExpression(expression, trueStatement as IExpressionSyntax, falseStatement as IExpressionSyntax, parent);
        }

        public override ISyntaxNode VisitLiteral(ILiteralOperation operation, ISyntaxNode parent)
        {
            var value = "";
            if (operation.ConstantValue.HasValue) value = operation.ConstantValue.ToString();
            if (operation.Type.IsEquivalentTo(typeof(string)))
            {
                value = $"'{value}'";
            }

            if (operation.Type.IsEquivalentTo(typeof(bool)))
            {
                if ((bool)operation.ConstantValue.Value)
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
            }
            return _factory.CreateLiteralExpression(value, parent);
        }

        public override ISyntaxNode VisitInvocation(IInvocationOperation operation, ISyntaxNode parent)
        {
            var method = operation.TargetMethod;
            var arguments = new List<IExpressionSyntax>();
            var reference = operation.Instance.Accept(this, parent) as IReferenceSyntax;
            foreach (var argument in operation.Arguments)
            {
                arguments.Add(argument.Accept(this, parent) as IExpressionSyntax);
            }
            if (method.HasAttribute<ClientSideMethodAttribute>())
            {
                var identifier = _factory.CreateIdentifier($"{method.Name}", parent);
                return _factory.CreateMethodCall(reference, identifier, arguments.ToImmutableList(), parent);
            }
            else
            {
                var methodTranslator = _methodTranslatorRegistry.FindRegisteredMethod(operation.TargetMethod);
                if (methodTranslator != null)
                {
                    return methodTranslator.Translate(operation, arguments, reference, parent);
                }
                throw new InvalidOperationException("You can't call methods without ClientSideMethod attribute");
            }
        }

        public override ISyntaxNode VisitParameterReference(IParameterReferenceOperation operation, ISyntaxNode parent)
        {
            var identifier = _factory.CreateIdentifier(operation.Parameter.Name, parent);
            return _factory.CreateLocalVariableReference(identifier, parent);
        }

        public override ISyntaxNode VisitInstanceReference(IInstanceReferenceOperation operation, ISyntaxNode parent)
        {
            return _factory.CreateInstanceReference(parent);
        }

        public override ISyntaxNode VisitArgument(IArgumentOperation operation, ISyntaxNode parent)
        {
            return operation.Value.Accept(this, parent);
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
                expression = new TsMethodCallSyntax( parent, methodIdentifier, parameters.ToImmutableList(), null);
            }

            var assignment = _factory.CreateAssignment(identifier, expression, parent);
            return assignment;
        }

        public override ISyntaxNode VisitUnaryOperator(IUnaryOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating unary operation.");
            var operand = operation.Operand.Accept(this, parent) as IExpressionSyntax;
            var unaryOperator = operation.OperatorKind.ToTsUnaryOperator();
            return _factory.CreateUnaryOperation(operand, unaryOperator, parent);
        }

        public override ISyntaxNode VisitBinaryOperator(IBinaryOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating binary operation.");
            var left = operation.LeftOperand.Accept(this, parent) as IExpressionSyntax;
            if (operation.LeftOperand is IBinaryOperation) left = _factory.CreateParenthesizedExpression(left, parent);
            var binaryOperator = operation.OperatorKind.ToTsBinaryOperator();
            var right = operation.RightOperand.Accept(this, parent) as IExpressionSyntax;
            if (operation.RightOperand is IBinaryOperation) right = _factory.CreateParenthesizedExpression(right, parent);
            return _factory.CreateBinaryOperation(left, binaryOperator, right, parent);
        }

        public override ISyntaxNode VisitConversion(IConversionOperation operation, ISyntaxNode argument)
        {
            return operation.Operand.Accept(this, argument);
        }

        public override ISyntaxNode VisitLocalReference(ILocalReferenceOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating local reference operation.");
            var identifier = _factory.CreateIdentifier(operation.Local.Name, parent);
            return _factory.CreateLocalVariableReference(identifier, parent);
        }

        public override ISyntaxNode VisitPropertyReference(IPropertyReferenceOperation operation, ISyntaxNode parent)
        {
            _logger.LogDebug("Operations", "Translating property reference operation.");
            var reference = operation.Instance.Accept(this, parent) as IReferenceSyntax;
            var identifier = _factory.CreateIdentifier(operation.Property.Name, parent);
            return _factory.CreatePropertyReferenceSyntax(reference, identifier, parent);
        }
    }
}
