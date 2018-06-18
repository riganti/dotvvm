using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Exceptions;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Translators.Builtin;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace DotVVM.TypeScript.Compiler.Translators.Operations
{
    
    internal class OperationTranslatingVisitor : OperationVisitor<ISyntaxNode, ISyntaxNode>
    {
        private readonly ILogger _logger;
        private readonly ISyntaxFactory _factory;
        private readonly IBuiltinMethodTranslatorRegistry _methodTranslatorRegistry;
        private readonly IBuiltinPropertyTranslatorRegistry _propertyTranslatorRegistry;

        public OperationTranslatingVisitor(ILogger logger, ISyntaxFactory factory, IBuiltinMethodTranslatorRegistry methodTranslatorRegistry, IBuiltinPropertyTranslatorRegistry propertyTranslatorRegistry)
        {
            _logger = logger;
            _factory = factory;
            _methodTranslatorRegistry = methodTranslatorRegistry;
            _propertyTranslatorRegistry = propertyTranslatorRegistry;
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
            var reference = operation.Instance?.Accept(this, parent) as IReferenceSyntax;
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
                var methodTranslator = _methodTranslatorRegistry.FindRegisteredTranslator(operation.TargetMethod);
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
            var arguments = new List<IExpressionSyntax>();
            foreach (var argument in operation.Arguments)
            {
                var argumentExpression = argument.Accept(this, parent) as IExpressionSyntax;
                arguments.Add(argumentExpression);
            }
            var identifier = _factory.CreateIdentifier(operation.Property.Name, parent);
            var translator = _propertyTranslatorRegistry.FindRegisteredTranslator(operation.Property);
            if (translator != null)
            {
                return translator.Translate(reference, operation.Property, parent, arguments);
            }
            else if (operation.Property.IsIndexer)
            {
                return _factory.CreateArrayElementReference(reference, arguments.Single(), parent);
            }
            else
            {
                return _factory.CreatePropertyReferenceSyntax(reference, identifier, parent, operation.Property.Type);
            }
        }

        public override ISyntaxNode VisitObjectCreation(IObjectCreationOperation operation, ISyntaxNode parent)
        {
            var typeSyntax = _factory.CreateType(operation.Type, parent);
            var arguments = new List<IExpressionSyntax>();
            foreach (var operationArgument in operation.Arguments)
            {
                arguments.Add(operationArgument.Accept(this, parent) as IExpressionSyntax);
            }
            return _factory.CreateObjectCreationExpression(typeSyntax, arguments, parent);
        }

        public override ISyntaxNode VisitForEachLoop(IForEachLoopOperation operation, ISyntaxNode parent)
        {
            var variable = operation.LoopControlVariable.Accept(this, parent) as IExpressionSyntax;
            var collection = operation.Collection.Accept(this, parent) as IReferenceSyntax;
            var body = operation.Body.Accept(this, parent) as IStatementSyntax;
            return _factory.CreateForEachLoopStatement(variable, collection, body, parent);
        }

        private void ThrowUnsupportedException(IOperation operation)
        {
            var linePositionSpan = operation.Syntax.SyntaxTree.GetLineSpan(operation.Syntax.Span);
            var filePath = operation.Syntax.SyntaxTree.FilePath;
            throw new NotSupportedOperationException(filePath, linePositionSpan, operation.Kind);
        }

        public override ISyntaxNode VisitSwitch(ISwitchOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitSwitchCase(ISwitchCaseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitRelationalCaseClause(IRelationalCaseClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitRangeCaseClause(IRangeCaseClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitForToLoop(IForToLoopOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitLabeled(ILabeledOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitBranch(IBranchOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitEmpty(IEmptyOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitLock(ILockOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitTry(ITryOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitCatchClause(ICatchClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitUsing(IUsingOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitStop(IStopOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitEnd(IEndOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitOmittedArgument(IOmittedArgumentOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitArrayElementReference(IArrayElementReferenceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitFieldReference(IFieldReferenceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitMethodReference(IMethodReferenceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitEventReference(IEventReferenceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitEventAssignment(IEventAssignmentOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitConditionalAccess(IConditionalAccessOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitCoalesce(ICoalesceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitIsType(IIsTypeOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitSizeOf(ISizeOfOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitTypeOf(ITypeOfOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitAnonymousFunction(IAnonymousFunctionOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDelegateCreation(IDelegateCreationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitAwait(IAwaitOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitNameOf(INameOfOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitThrow(IThrowOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitAddressOf(IAddressOfOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDynamicInvocation(IDynamicInvocationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitMemberInitializer(IMemberInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitCollectionElementInitializer(ICollectionElementInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitFieldInitializer(IFieldInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitPropertyInitializer(IPropertyInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitParameterInitializer(IParameterInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitArrayCreation(IArrayCreationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitArrayInitializer(IArrayInitializerOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDeclarationExpression(IDeclarationExpressionOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitCompoundAssignment(ICompoundAssignmentOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitParenthesized(IParenthesizedOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDefaultValue(IDefaultValueOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitInvalid(IInvalidOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitLocalFunction(ILocalFunctionOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitInterpolatedString(IInterpolatedStringOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitInterpolation(IInterpolationOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitIsPattern(IIsPatternOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitConstantPattern(IConstantPatternOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitDeclarationPattern(IDeclarationPatternOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitPatternCaseClause(IPatternCaseClauseOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitTuple(ITupleOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitTranslatedQuery(ITranslatedQueryOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }

        public override ISyntaxNode VisitRaiseEvent(IRaiseEventOperation operation, ISyntaxNode argument)
        {
            ThrowUnsupportedException(operation);
            return null;
        }
    }
}
