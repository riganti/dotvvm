using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Utils;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Inference;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DotVVM.Framework.Compilation.Parser.Binding.Parser.Annotations;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using FastExpressionCompiler;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace DotVVM.Framework.Compilation.Binding
{
    public class ExpressionBuildingVisitor : BindingParserNodeVisitor<Expression>
    {
        public TypeRegistry Registry { get; set; }
        public Expression? Scope { get; set; }
        /// <summary> We use the parser to parse directives where only type name is expected. At that place, the flag is set to true otherwise it's false </summary>
        public bool ResolveOnlyTypeName { get; set; }
        public Type? ExpectedType { get; set; }
        public ImmutableDictionary<string, ParameterExpression> Variables { get; set; } =
            ImmutableDictionary<string, ParameterExpression>.Empty;

        private TypeInferer inferer;
        private int expressionDepth;
        private List<ExceptionDispatchInfo>? currentErrors;
        private readonly MemberExpressionFactory memberExpressionFactory;

        public ExpressionBuildingVisitor(TypeRegistry registry, MemberExpressionFactory memberExpressionFactory, Type? expectedType = null)
        {
            Registry = registry;
            ExpectedType = expectedType;
            this.memberExpressionFactory = memberExpressionFactory;
            this.inferer = new TypeInferer();
        }

        [return: MaybeNull]
        protected T HandleErrors<T, TNode>(TNode node, Func<TNode, T> action, string defaultErrorMessage = "Binding compilation failed", bool allowResultNull = true)
            where TNode : BindingParserNode
        {
            T result = default!;
            try
            {
                result = action(node);
            }
            catch (BindingCompilationException exception)
            {
                AddError(exception);
            }
            catch (Exception exception)
            {
                AddError(new BindingCompilationException(defaultErrorMessage, exception, node));
            }
            if (!allowResultNull && result == null)
            {
                AddError(new BindingCompilationException(defaultErrorMessage, node));
            }
            return result;
        }

        protected void AddError(params Exception[] errors)
        {
            currentErrors ??= [];
            foreach (var e in errors)
                currentErrors.Add(ExceptionDispatchInfo.Capture(e));
        }

        protected void ThrowOnErrors()
        {
            if (currentErrors != null && currentErrors.Count > 0)
            {
                var currentErrors = this.currentErrors;
                this.currentErrors = null;
                if (currentErrors.Count == 1)
                {
                    currentErrors[0].Throw();
                }
                throw new AggregateException(currentErrors.Select(e => e.SourceException));
            }
        }

        public override Expression Visit(BindingParserNode node)
        {
            var regBackup = Registry;
            var errors = currentErrors;
            try
            {
                expressionDepth++;
                ThrowIfNotTypeNameRelevant(node);
                return base.Visit(node);
            }
            catch (Exception e) when (e is not BindingCompilationException)
            {
                throw new BindingCompilationException(e.Message, e, node);
            }
            finally
            {
                currentErrors = errors;
                Registry = regBackup;
                expressionDepth--;
            }
        }

        protected override Expression VisitLiteralExpression(LiteralExpressionBindingParserNode node)
        {
            return Expression.Constant(node.Value);
        }

        protected override Expression VisitInterpolatedStringExpression(InterpolatedStringBindingParserNode node)
        {
            var target = new MethodGroupExpression(
                new StaticClassIdentifierExpression(typeof(string)),
                nameof(String.Format)
            );

            if (node.Arguments.Any())
            {
                // Translate to a String.Format(...) call
                var arguments = node.Arguments.Select((arg, index) => HandleErrors(node.Arguments[index], Visit)!).ToArray();
                ThrowOnErrors();
                return memberExpressionFactory.Call(target, new[] { Expression.Constant(node.Format) }.Concat(arguments).ToArray());
            }
            else
            {
                // There are no interpolation expressions - we can just return string
                return Expression.Constant(node.Format);
            }
        }

        protected override Expression VisitParenthesizedExpression(ParenthesizedExpressionBindingParserNode node)
        {
            // just visit content
            return Visit(node.InnerExpression);
        }

        protected override Expression VisitUnaryOperator(UnaryOperatorBindingParserNode node)
        {
            inferer.BeginNoInference();
            var operand = Visit(node.InnerExpression);
            inferer.PopContext();

            ExpressionType eop;
            switch (node.Operator)
            {
                case BindingTokenType.AddOperator:
                    eop = ExpressionType.UnaryPlus;
                    break;
                case BindingTokenType.SubtractOperator:
                    eop = ExpressionType.Negate;
                    break;
                case BindingTokenType.NotOperator:
                    eop = ExpressionType.Not;
                    break;
                case BindingTokenType.OnesComplementOperator:
                    eop = ExpressionType.OnesComplement;
                    break;
                default:
                    throw new NotSupportedException($"unary operator { node.Operator } is not supported");
            }
            return memberExpressionFactory.GetUnaryOperator(operand, eop);
        }

        protected override Expression VisitBinaryOperator(BinaryOperatorBindingParserNode node)
        {
            ExpressionType eop;
            switch (node.Operator)
            {
                case BindingTokenType.AddOperator:
                    eop = ExpressionType.Add;
                    break;
                case BindingTokenType.SubtractOperator:
                    eop = ExpressionType.Subtract;
                    break;
                case BindingTokenType.MultiplyOperator:
                    eop = ExpressionType.Multiply;
                    break;
                case BindingTokenType.DivideOperator:
                    eop = ExpressionType.Divide;
                    break;
                case BindingTokenType.ModulusOperator:
                    eop = ExpressionType.Modulo;
                    break;
                case BindingTokenType.EqualsEqualsOperator:
                    eop = ExpressionType.Equal;
                    break;
                case BindingTokenType.LessThanOperator:
                    eop = ExpressionType.LessThan;
                    break;
                case BindingTokenType.LessThanEqualsOperator:
                    eop = ExpressionType.LessThanOrEqual;
                    break;
                case BindingTokenType.GreaterThanOperator:
                    eop = ExpressionType.GreaterThan;
                    break;
                case BindingTokenType.GreaterThanEqualsOperator:
                    eop = ExpressionType.GreaterThanOrEqual;
                    break;
                case BindingTokenType.NotEqualsOperator:
                    eop = ExpressionType.NotEqual;
                    break;
                case BindingTokenType.NullCoalescingOperator:
                    eop = ExpressionType.Coalesce;
                    break;
                case BindingTokenType.AndOperator:
                    eop = ExpressionType.And;
                    break;
                case BindingTokenType.AndAlsoOperator:
                    eop = ExpressionType.AndAlso;
                    break;
                case BindingTokenType.OrOperator:
                    eop = ExpressionType.Or;
                    break;
                case BindingTokenType.OrElseOperator:
                    eop = ExpressionType.OrElse;
                    break;
                case BindingTokenType.ExclusiveOrOperator:
                    eop = ExpressionType.ExclusiveOr;
                    break;
                case BindingTokenType.AssignOperator:
                    eop = ExpressionType.Assign;
                    break;
                default:
                    throw new NotSupportedException($"unary operator { node.Operator } is not supported");
            }


            inferer.BeginNoInference();
            var left = HandleErrors(node.FirstExpression, Visit);
            inferer.PopContext();

            if (node.Operator is BindingTokenType.AssignOperator or BindingTokenType.NullCoalescingOperator)
                inferer.BeginExplicitTypeInference(left!.Type);
            else
                inferer.BeginNoInference();

            var right = HandleErrors(node.SecondExpression, Visit);

            inferer.PopContext();

            ThrowOnErrors();

            return memberExpressionFactory.GetBinaryOperator(left!, right!, eop);
        }

        protected override Expression VisitArrayAccess(ArrayAccessBindingParserNode node)
        {
            var target = HandleErrors(node.TargetExpression, Visit);
            var index = HandleErrors(node.ArrayIndexExpression, Visit);
            ThrowOnErrors();

            return ExpressionHelper.GetIndexer(target!, index!);
        }

        protected override Expression VisitFunctionCall(FunctionCallBindingParserNode node)
        {
            inferer.BeginNoInference();
            var target = HandleErrors(node.TargetExpression, Visit);
            inferer.PopContext();
            var args = new Expression[node.ArgumentExpressions.Count];

            inferer.BeginFunctionCall(target as MethodGroupExpression, args.Length);

            var lambdaNodeIndices = new List<int>();
            // Initially process all nodes that are not lambdas
            for (var i = 0; i < args.Length; i++)
            {
                if (node.ArgumentExpressions[i] is LambdaBindingParserNode)
                {
                    lambdaNodeIndices.Add(i);
                    continue;
                }

                inferer.SetProbedArgumentIndex(i);
                args[i] = HandleErrors(node.ArgumentExpressions[i], Visit)!;
                inferer.SetArgument(args[i], i);
            }
            // Subsequently process all lambdas
            foreach (var index in lambdaNodeIndices)
            {
                inferer.SetProbedArgumentIndex(index);
                args[index] = HandleErrors(node.ArgumentExpressions[index], Visit)!;
                inferer.SetArgument(args[index], index);
            }

            inferer.PopContext();
            ThrowOnErrors();

            return memberExpressionFactory.Call(target!, args);
        }

        protected override Expression VisitSimpleName(SimpleNameBindingParserNode node)
        {
            return GetMemberOrTypeExpression(node, null) ?? Expression.Default(typeof(void));
        }

        protected override Expression VisitPredefinedTypeName(PredefinedTypeBindingParserNode node)
        {
            var type = node.NameToken.Type switch {
                BindingTokenType.KeywordBool => typeof(bool),
                BindingTokenType.KeywordByte => typeof(byte),
                BindingTokenType.KeywordChar => typeof(char),
                BindingTokenType.KeywordDecimal => typeof(decimal),
                BindingTokenType.KeywordDouble => typeof(double),
                BindingTokenType.KeywordFloat => typeof(float),
                BindingTokenType.KeywordInt => typeof(int),
                BindingTokenType.KeywordLong => typeof(long),
                BindingTokenType.KeywordObject => typeof(object),
                BindingTokenType.KeywordSbyte => typeof(sbyte),
                BindingTokenType.KeywordShort => typeof(short),
                BindingTokenType.KeywordString => typeof(string),
                BindingTokenType.KeywordUint => typeof(uint),
                BindingTokenType.KeywordUlong => typeof(ulong),
                BindingTokenType.KeywordUshort => typeof(ushort),
                BindingTokenType.KeywordVoid => typeof(void),
                _ => throw new NotSupportedException($"Predefined type {node.Name} is not supported")
            };
            return new StaticClassIdentifierExpression(type);
        }

        protected override Expression VisitAssemblyQualifiedName(AssemblyQualifiedNameBindingParserNode node)
        {
            if (node.AssemblyName.HasNodeErrors)
            {
                var message = node.AssemblyName.NodeErrors.StringJoin(Environment.NewLine);
                throw new BindingCompilationException(message, node.AssemblyName);
            }

            // Assembly was already added to TypeRegistry - we can just visit the type name
            return Visit(node.TypeName);
        }

        protected override Expression VisitConditionalExpression(ConditionalExpressionBindingParserNode node)
        {
            var condition = HandleErrors(node.ConditionExpression, n => TypeConversion.EnsureImplicitConversion(Visit(n), typeof(bool)));
            var trueExpr = HandleErrors(node.TrueExpression, Visit)!;
            var falseExpr = HandleErrors(node.FalseExpression, Visit)!;
            ThrowOnErrors();

            if (trueExpr.Type != falseExpr.Type)
            {
                // implicit conversions are specified at https://github.com/ljw1004/csharpspec/blob/gh-pages/expressions.md#conditional-operator
                // > If x has type X and y has type Y then
                // > * If an implicit conversion (Implicit conversions) exists from X to Y, but not from Y to X, then Y is the type of the conditional expression.
                // > * If an implicit conversion (Implicit conversions) exists from Y to X, but not from X to Y, then X is the type of the conditional expression.
                // > * Otherwise, no expression type can be determined, and a compile-time error occurs.
                // 

                var trueConverted = TypeConversion.ImplicitConversion(trueExpr, falseExpr.Type, allowToString: true);
                var falseConverted = TypeConversion.ImplicitConversion(falseExpr, trueExpr.Type, allowToString: true);

                if (trueConverted is null && falseConverted is null)
                    throw new BindingCompilationException($"Type of conditional expression '{node.ToDisplayString()}' cannot be determined because there is no implicit conversion between '{trueExpr.Type.ToCode()}' and '{falseExpr.Type.ToCode()}'", node);
                else if (trueConverted is null)
                    falseExpr = falseConverted;
                else if (falseConverted is null)
                    trueExpr = trueConverted;
                else
                {
                    Debug.Assert(trueConverted.Type != falseConverted.Type);
                    // 1. We represent some "typeless expressions" as expression of type object
                    // 2. We allow conversion to string and also have the implicit conversion from string literal to enum
                    // 3. For some reason, we allow T? -> T implicit conversion as well as T -> T?, this will prefer the nullable type
                    // -> if we have an ambiguity, try to solve it by preferring the more specific type

                    if (trueConverted.Type == typeof(object))
                        falseExpr = falseConverted;
                    else if (falseConverted.Type == typeof(object))
                        trueExpr = trueConverted;
                    else if (trueConverted.Type == typeof(string))
                        falseExpr = falseConverted;
                    else if (falseConverted.Type == typeof(string))
                        trueExpr = trueConverted;
                    else if (trueConverted.Type.UnwrapNullableType() == falseConverted.Type.UnwrapNullableType())                    
                    {
                        if (trueConverted.Type.IsNullable())
                            falseExpr = falseConverted;
                        else
                            trueExpr = trueConverted;
                    }
                    else
                        throw new BindingCompilationException($"Type of conditional expression '{node.ToDisplayString()}' cannot be determined because because '{trueExpr.Type.ToCode()}' and '{falseExpr.Type.ToCode()}' implicitly convert to one another", node);
                }
            }

            return Expression.Condition(condition!, trueExpr.NotNull(), falseExpr.NotNull());
        }

        protected override Expression VisitMemberAccess(MemberAccessBindingParserNode node)
        {
            var nameNode = node.MemberNameExpression;
            var typeParameters = nameNode is GenericNameBindingParserNode
                ? ResolveGenericArguments(nameNode.CastTo<GenericNameBindingParserNode>().TypeArguments)
                : null;
            var identifierName = (typeParameters?.Length ?? 0) > 0
                ? $"{nameNode.Name}`{typeParameters!.Length}"
                : nameNode.Name;

            inferer.BeginNoInference();
            var target = Visit(node.TargetExpression);
            inferer.PopContext();

            if (target is UnknownStaticClassIdentifierExpression unknownClass)
            {
                var name = unknownClass.Name + "." + identifierName;

                var resolvedTypeExpression = Registry.Resolve(name, throwOnNotFound: false) ?? new UnknownStaticClassIdentifierExpression(name, node);

                if (typeParameters != null)
                {
                    var resolvedType = resolvedTypeExpression.Type.MakeGenericType(typeParameters);
                    resolvedTypeExpression = new StaticClassIdentifierExpression(resolvedType);
                }
                return resolvedTypeExpression;
            }

            // we try to resolve member access into an extension parameter
            // for example _parent._index should resolve into the _index extension parameter on the parent context
            var extensionParameter = TryResolveExtensionParameter(target, nameNode);

            // even when we find the extension parameter, member properties should have priority (for compatibility, at least)

            return
                memberExpressionFactory.GetMember(
                    target, nameNode.Name, typeParameters,
                    throwExceptions: extensionParameter is null,
                    onlyMemberTypes: ResolveOnlyTypeName
                ) ?? extensionParameter!;
        }

        Expression? TryResolveExtensionParameter(Expression target, IdentifierNameBindingParserNode nameNode)
        {
            // target is _parent, _this, _root, ...
            if (target.GetParameterAnnotation() is BindingParameterAnnotation { ExtensionParameter: null, DataContext: { } dataContext } &&
                // name is simple (no generics)
                nameNode is SimpleNameBindingParserNode { Name: { } name } &&
                dataContext.ExtensionParameters.FirstOrDefault(e => e.Identifier == name) is { } parameter)
            {
                return Expression.Parameter(
                    ResolvedTypeDescriptor.ToSystemType(parameter.ParameterType) ?? typeof(UnknownTypeSentinel),
                    parameter.Identifier
                ).AddParameterAnnotation(new BindingParameterAnnotation(dataContext, parameter));
            }
            return null;
        }

        protected override Expression VisitGenericName(GenericNameBindingParserNode node)
        {
            var parameters = ResolveGenericArguments(node.TypeArguments);
            return GetMemberOrTypeExpression(node, parameters) ?? Expression.Default(typeof(void));
        }

        protected override Expression VisitTypeReference(TypeReferenceBindingParserNode node)
        {
            if (node is ActualTypeReferenceBindingParserNode actualType)
            {
                return Visit(actualType.Type);
            }
            else if (node is NullableTypeReferenceBindingParserNode nullableType)
            {
                var innerTypeExpr = Visit(nullableType.InnerType);
                if (!innerTypeExpr.Type.IsValueType)
                    throw new BindingCompilationException($"Wrapping {innerTypeExpr.Type} as nullable is not supported!", node);

                return new StaticClassIdentifierExpression(innerTypeExpr.Type.MakeNullableType());
            }
            else if (node is ArrayTypeReferenceBindingParserNode arrayType)
            {
                var elementTypeExpr = Visit(arrayType.ElementType);
                return new StaticClassIdentifierExpression(elementTypeExpr.Type.MakeArrayType());
            }
            else if (node is GenericTypeReferenceBindingParserNode genericType)
            {
                var identifierName = $"{genericType.Type.ToDisplayString()}`{genericType.Arguments.Count()}";
                var parameters = ResolveGenericArguments(genericType.Arguments);

                var resolvedTypeExpr = Registry.Resolve(identifierName, throwOnNotFound: false) ?? new UnknownStaticClassIdentifierExpression(identifierName, node);
                return new StaticClassIdentifierExpression(resolvedTypeExpr.Type.MakeGenericType(parameters));
            }

            throw new DotvvmCompilationException($"Unknown type reference binding node {node.GetType()}!");
        }

        protected override Expression VisitLambda(LambdaBindingParserNode node)
        {
            // Create lambda definition
            var lambdaParameters = new ParameterExpression[node.ParameterExpressions.Count];

            // Apply information from type inference if available
            var hintType = (expressionDepth == 1) ? ExpectedType : null;
            var typeInferenceData = inferer.Infer(hintType).Lambda(node.ParameterExpressions.Count);
            if (typeInferenceData.Result)
            {
                for (var paramIndex = 0; paramIndex < typeInferenceData.Parameters!.Length; paramIndex++)
                {
                    var currentParamType = typeInferenceData.Parameters[paramIndex];
                    if (currentParamType.ContainsGenericParameters)
                        throw new BindingCompilationException($"Internal bug: lambda parameter still contains generic arguments: parameters[{paramIndex}] = {currentParamType.ToCode()}", node);
                    node.ParameterExpressions[paramIndex].SetResolvedType(currentParamType);
                }
            }

            for (var i = 0; i < lambdaParameters.Length; i++)
                lambdaParameters[i] = (ParameterExpression)Visit(node.ParameterExpressions[i]);

            // Make sure that parameter identifiers are distinct
            if (lambdaParameters.GroupBy(param => param.Name).Any(group => group.Count() > 1))
                throw new BindingCompilationException("Parameter identifiers must be unique.", node);

            // Make sure that parameter identifiers do not collide with existing symbols within registry
            var collision = lambdaParameters.FirstOrDefault(param => Registry.Resolve(param.Name!, false) != null);
            if (collision != null)
            {
                throw new BindingCompilationException($"Identifier \"{collision.Name}\" is already in use. Choose a different " +
                    $"identifier for the parameter with index {Array.IndexOf(lambdaParameters, collision)}.", node);
            }

            // Register lambda parameters as new symbols
            Registry = Registry.AddSymbols(lambdaParameters);

            // Create lambda body
            var body = Visit(node.BodyExpression);

            ThrowOnErrors();
            return CreateLambdaExpression(body, lambdaParameters, typeInferenceData.Type);
        }

        protected override Expression VisitLambdaParameter(LambdaParameterBindingParserNode node)
        {
            if (node.Type == null && node.ResolvedType == null)
                throw new BindingCompilationException($"Could not infer type of parameter.", node);

            if (node.ResolvedType != null)
            {
                // Type was not specified but was inferred
                return Expression.Parameter(node.ResolvedType, node.Name.Name);
            }
            else
            {
                // Type was specified and needs to be obtained from binding node
                var parameterType = Visit(node.Type!).Type;
                return Expression.Parameter(parameterType, node.Name.Name);
            }
        }

        private Expression CreateLambdaExpression(Expression body, ParameterExpression[] parameters, Type? delegateType)
        {
            if (delegateType is null || delegateType == typeof(object) || delegateType == typeof(Delegate))
                // Assume delegate is a System.Func<...>
                return Expression.Lambda(body, parameters);

            if (!delegateType.IsDelegate(out var invokeMethod))
                throw new DotvvmCompilationException($"Cannot create lambda function, type '{delegateType.ToCode()}' is not a delegate type.");

            if (invokeMethod.ReturnType == typeof(void))
            {
                // We must validate that lambda body contains a valid statement
                if ((body.NodeType != ExpressionType.Default) && (body.NodeType != ExpressionType.Block) && (body.NodeType != ExpressionType.Call) && (body.NodeType != ExpressionType.Assign))
                    throw new DotvvmCompilationException($"Only method invocations and assignments can be used as statements.");

                // Make sure the result type will be void by adding an empty expression
                body = Expression.Block(body, Expression.Empty());
            }

            // convert body result to the delegate return type
            if (invokeMethod.ReturnType.ContainsGenericParameters)
            {
                if (invokeMethod.ReturnType.IsGenericType)
                {
                    // no fancy implicit conversions are supported, only inheritance
                    if (!ReflectionUtils.IsAssignableToGenericType(body.Type, invokeMethod.ReturnType.GetGenericTypeDefinition(), out var bodyReturnType))
                    {
                        throw new DotvvmCompilationException($"Cannot convert lambda function body of type '{body.Type.ToCode()}' to the delegate return type '{invokeMethod.ReturnType.ToCode()}'.");
                    }
                    else
                    {
                        body = Expression.Convert(body, bodyReturnType);
                    }
                }
                else
                {
                    // fine, we will unify it in the next step

                    // Some complex conversions like Tuple<T, List<object>> -> Tuple<T, IEnumerable<T2>>
                    // will fail, but we don't have to support everything
                }
            }
            else
            {
                body = TypeConversion.EnsureImplicitConversion(body, invokeMethod.ReturnType);
            }

            if (delegateType.ContainsGenericParameters)
            {
                var delegateTypeDef = delegateType.GetGenericTypeDefinition();
                // The delegate is either purely generic (Func<T, T>) or only some of the generic arguments are known (Func<T, bool>)
                // initialize generic args with the already known types
                var genericArgs =
                    delegateTypeDef.GetGenericArguments().Zip(
                        delegateType.GetGenericArguments(),
                        (param, argument) => new KeyValuePair<Type, Type>(param, argument)
                    )
                    .Where(p => p.Value != p.Key)
                    .ToDictionary(p => p.Key, p => p.Value);

                var delegateParameters = invokeMethod.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!ReflectionUtils.TryUnifyGenericTypes(delegateParameters[i].ParameterType, parameters[i].Type, genericArgs))
                    {
                        throw new DotvvmCompilationException($"Could not match lambda function parameter '{parameters[i].Type.ToCode()} {parameters[i].Name}' to delegate parameter '{delegateParameters[i].ParameterType.ToCode()} {delegateParameters[i].Name}'.");
                    }
                }
                if (!ReflectionUtils.TryUnifyGenericTypes(invokeMethod.ReturnType, body.Type, genericArgs))
                {
                    throw new DotvvmCompilationException($"Could not match lambda function return type '{body.Type.ToCode()}' to delegate return type '{invokeMethod.ReturnType.ToCode()}'.");
                }
                ReflectionUtils.ExpandUnifiedTypes(genericArgs);

                if (!delegateTypeDef.GetGenericArguments().All(a => genericArgs.TryGetValue(a, out var v) && !v.ContainsGenericParameters))
                {
                    var missingGenericArgs = delegateTypeDef.GetGenericArguments().Where(genericArg => !genericArgs.ContainsKey(genericArg) || genericArgs[genericArg].ContainsGenericParameters);
                    throw new DotvvmCompilationException($"Could not infer all generic arguments ({string.Join(", ", missingGenericArgs)}) of delegate type '{delegateType.ToCode()}' from lambda expression '({string.Join(", ", parameters.Select(p => $"{p.Type.ToCode()} {p.Name}"))}) => ...'.");
                }

                delegateType = delegateTypeDef.MakeGenericType(
                    delegateTypeDef.GetGenericArguments().Select(genericParam => genericArgs[genericParam]).ToArray()
                );
            }

            return Expression.Lambda(delegateType, body, parameters);
        }

        protected override Expression VisitBlock(BlockBindingParserNode node)
        {
            var left = HandleErrors(node.FirstExpression, Visit);

            var originalVariables = this.Variables;
            ParameterExpression? variable = null;
            if (node.Variable is object)
            {
                ThrowOnErrors(); // cannot infer variable type
                variable = Expression.Parameter(left!.Type, node.Variable.Name);
                this.Variables = this.Variables.SetItem(node.Variable.Name, variable);

                left = Expression.Assign(variable, left);
            }

            var right = HandleErrors(node.SecondExpression, Visit)!;

            this.Variables = originalVariables;
            ThrowOnErrors();

            if (typeof(Task).IsAssignableFrom(left!.Type))
            {
                if (variable is object)
                    throw new NotImplementedException("Variable definition of type Task is not supported.");
                return ExpressionHelper.RewriteTaskSequence(left, right);
            }

            var variables = variable is null ? Array.Empty<ParameterExpression>() : new [] { variable };
            if (right is BlockExpression rightBlock)
            {
                // flat the `(a; b; c; d; e; ...)` expression down
                return Expression.Block(variables.Concat(rightBlock.Variables), new Expression[] { left }.Concat(rightBlock.Expressions));
            }
            else return Expression.Block(variables, left, right);
        }

        protected override Expression VisitFormattedExpression(FormattedBindingParserNode node)
        {
            var target = new MethodGroupExpression(
                new StaticClassIdentifierExpression(typeof(string)),
                nameof(String.Format)
            );

            var nodeObj = Visit(node.Node);
            return memberExpressionFactory.Call(target, new[] { Expression.Constant(node.Format), nodeObj });
        }

        protected override Expression VisitVoid(VoidBindingParserNode node) => Expression.Default(typeof(void));

        protected override Expression VisitArrayInitializer(ArrayInitializerExpression node)
        {
            var initializers = node.ElementInitializers.Select(e => HandleErrors(e, Visit)).ToArray();
            ThrowOnErrors();

            var firstInitializer = initializers.FirstOrDefault();

            var firstElementType = firstInitializer?.Type ?? throw new BindingCompilationException($"Could not get the determine type of array element.", node.ElementInitializers.FirstOrDefault() ?? node);

            var arrayElementType = initializers.All(i => i!.Type.IsAssignableFrom(firstElementType)) ? firstElementType : throw new BindingCompilationException($"All elements of the array initializer must be of the same type.", node);

            return Expression.NewArrayInit(arrayElementType, initializers.Select(i => Expression.Convert(i!, arrayElementType)));
        }

        private Expression? GetMemberOrTypeExpression(IdentifierNameBindingParserNode node, Type[]? typeParameters)
        {
            var name = node.Name;
            if (string.IsNullOrWhiteSpace(name)) return null;
            var expr = getExpression();

            if (expr is null) return new UnknownStaticClassIdentifierExpression(name, node);
            if (expr is ParameterExpression && expr.Type == typeof(UnknownTypeSentinel)) throw new BindingCompilationException($"Type of '{expr}' could not be resolved.", node);
            return expr;

            Expression? getExpression()
            {
                if (Variables.TryGetValue(name, out var variable))
                    return variable;
                if (Scope is object && memberExpressionFactory.GetMember(Scope, node.Name, typeParameters, throwExceptions: false, onlyMemberTypes: ResolveOnlyTypeName, disableExtensionMethods: true) is Expression scopeMember)
                    return scopeMember;
                if (Registry.Resolve(node.Name, throwOnNotFound: false) is { } resolvedType)
                    return resolvedType;

                if (!node.IsEscapedKeyword)
                {
                    if (name == "nuint")
                        return new StaticClassIdentifierExpression(typeof(nuint));
                    if (name == "nint")
                        return new StaticClassIdentifierExpression(typeof(nint));
                }
                return null;
            }
        }

        private Type[] ResolveGenericArguments(List<TypeReferenceBindingParserNode> arguments)
        {
            var resolvedArguments = new Type[arguments.Count];

            for (var i = 0; i < arguments.Count; i++)
            {
                var typeArgument = arguments[i];
                resolvedArguments[i] = Visit(typeArgument).Type;
            }
            return resolvedArguments;
        }

        private void ThrowIfNotTypeNameRelevant(BindingParserNode node)
        {
            if (ResolveOnlyTypeName && !(node is MemberAccessBindingParserNode) && !(node is IdentifierNameBindingParserNode) && !(node is AssemblyQualifiedNameBindingParserNode) && !(node is TypeReferenceBindingParserNode) && !(node is TypeOrFunctionReferenceBindingParserNode))
            {
                throw new BindingCompilationException("Only type name is supported.", node);
            }
        }

        protected override Expression VisitConstructorCall(ConstructorCallBindingParserNode node)
        {
            Type? targetType;

            if (node.TypeExpression is null)
            {
                targetType = inferer.Infer((expressionDepth == 1) ? ExpectedType : null)
                                    .Constructor(node.ArgumentExpressions.Count).Type;

                if (targetType is null)
                    throw new BindingCompilationException($"Could not infer the constructed type of {node.ToDisplayString()}. Please specify the type name explicitly.", node);
            }
            else
            {
                var typeExpr = HandleErrors(node.TypeExpression, Visit);
                if (typeExpr is StaticClassIdentifierExpression classExpr)
                    targetType = classExpr.Type;
                else if (typeExpr is UnknownStaticClassIdentifierExpression unknownType)
                    throw unknownType.Error();
                else
                    throw new BindingCompilationException($"Cannot construct '{node.TypeExpression.ToDisplayString()}' ('{typeExpr}'), it's not a type reference.", node.TypeExpression);
            }

            var args = new Expression[node.ArgumentExpressions.Count];

            inferer.BeginConstructorCall(targetType, args.Length);

            var lambdaNodeIndices = new List<int>();
            // Initially process all nodes that are not lambdas
            for (var i = 0; i < args.Length; i++)
            {
                if (node.ArgumentExpressions[i] is LambdaBindingParserNode)
                {
                    lambdaNodeIndices.Add(i);
                    continue;
                }

                inferer.SetProbedArgumentIndex(i);
                args[i] = HandleErrors(node.ArgumentExpressions[i], Visit)!;
                inferer.SetArgument(args[i], i);
            }
            // Subsequently process all lambdas
            foreach (var index in lambdaNodeIndices)
            {
                inferer.SetProbedArgumentIndex(index);
                args[index] = HandleErrors(node.ArgumentExpressions[index], Visit)!;
                inferer.SetArgument(args[index], index);
            }

            inferer.PopContext();
            ThrowOnErrors();

            return memberExpressionFactory.Constructor(targetType, args);
        }

        protected override Expression VisitArrayConstruction(ArrayConstructionBindingParserNode node)
        {
            var size = node.Size.Select(x => HandleErrors(x, Visit)).ToArray();
            var typeExpr = node.ElementType is null ? null : HandleErrors(node.ElementType, Visit);
            var args = node.Initializers?.Select(e => HandleErrors(e, Visit)).ToArray();

            ThrowOnErrors();

            if (size.Length > 1)
                throw new BindingCompilationException("Multi-dimensional arrays are not supported.", node);

            if (size.Length == 0 && args is null)
                throw new BindingCompilationException("Array construction requires either a size expression or initializer expressions.", node);
            if (size.Length > 0 && args is {})
                throw new BindingCompilationException("Array construction cannot have both size and initializer expressions.", node.Size[0]);
            if (typeExpr is null && args is null or [])
                    throw new BindingCompilationException("No best type found for implicitly-typed array.", node);

            var elementType = (typeExpr as StaticClassIdentifierExpression)?.Type;
            if (typeExpr is { } && elementType is null)
                throw new BindingCompilationException($"'{node.ElementType!.ToDisplayString()}' is not a type reference.", node.ElementType);

            if (args is null)
            {
                var sizeConverted =
                    TypeConversion.ImplicitConversion(size[0]!, typeof(int)) ??
                    TypeConversion.ImplicitConversion(size[0]!, typeof(ulong)) ??
                    TypeConversion.EnsureImplicitConversion(size[0]!, typeof(long));

                return Expression.NewArrayBounds(elementType!, sizeConverted);
            }
            else
            {
                elementType ??= InferArrayElementType(args!);

                for (var i = 0; i < args.Length; i++)
                    args[i] = TypeConversion.EnsureImplicitConversion(args[i].NotNull(), elementType);

                return Expression.NewArrayInit(elementType, args!);
            }

            Type InferArrayElementType(Expression[] initExprs)
            {
                var firstType = initExprs[0].Type;

                var typeSet = initExprs.Select(expr => expr.Type).ToHashSet();
                if (typeSet.Count == 1)
                    return firstType; // all have same type

                var compatibleTypes = typeSet.Where(t => initExprs.All(expr => TypeConversion.ImplicitConversion(expr, t) != null)).Take(2).ToArray();

                if (compatibleTypes.Length == 1)
                    return compatibleTypes[0];

                if (compatibleTypes.Length == 0)
                    throw new Exception($"Cannot determine a common type for array initializer expressions: {string.Join(", ", typeSet.Select(e => e.ToCode()))}");
                if (compatibleTypes.Length > 1)
                    throw new Exception($"Multiple compatible types found for array initializer expressions: {string.Join(", ", compatibleTypes.Select(e => e.ToCode()))}");

                return compatibleTypes[0];
            }
        }
    }
}
