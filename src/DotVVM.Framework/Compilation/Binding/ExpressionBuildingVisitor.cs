using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Binding
{
    public class ExpressionBuildingVisitor : BindingParserNodeVisitor<Expression>
    {
        public TypeRegistry Registry { get; set; }
        public Expression Scope { get; set; }
        public bool ResolveOnlyTypeName { get; set; }

        private List<Exception> currentErrors;

        public ExpressionBuildingVisitor(TypeRegistry registry)
        {
            Registry = registry;
        }

        protected T HandleErrors<T, TNode>(TNode node, Func<TNode, T> action, string defaultErrorMessage = "Binding compilation failed", bool allowResultNull = true)
            where TNode : BindingParserNode
        {
            T result = default(T);
            try
            {
                result = action(node);
            }
            catch (BindingCompilationException exception)
            {
                if (currentErrors == null) currentErrors = new List<Exception>();
                currentErrors.Add(exception);
            }
            catch (Exception exception)
            {
                if (currentErrors == null) currentErrors = new List<Exception>();
                currentErrors.Add(new BindingCompilationException(defaultErrorMessage, exception, node));
            }
            if (!allowResultNull && result == null)
            {
                currentErrors.Add(new BindingCompilationException(defaultErrorMessage, node));
            }
            return result;
        }

        protected void AddError(params Exception[] errors)
        {
            if (currentErrors == null) currentErrors = new List<Exception>(errors);
            else currentErrors.AddRange(errors);
        }

        protected void ThrowOnErrors()
        {
            if (currentErrors != null && currentErrors.Count > 0)
            {
                var currentErrors = this.currentErrors;
                this.currentErrors = null;
                if (currentErrors.Count == 1)
                {
                    if (currentErrors[0].StackTrace == null
                        || (currentErrors[0] is BindingCompilationException && (currentErrors[0] as BindingCompilationException).Tokens == null)
                        || (currentErrors[0] is AggregateException && (currentErrors[0] as AggregateException).Message == null))
                        throw currentErrors[0];
                }
                throw new AggregateException(currentErrors);
            }
        }

        protected void RegisterSymbols(IEnumerable<KeyValuePair<string, Expression>> symbols)
        {
            Registry = Registry.AddSymbols(symbols);
        }

        public override Expression Visit(BindingParserNode node)
        {
            var regBackup = Registry;
            var errors = currentErrors;
            try
            {
                ThrowIfNotTypeNameRelevant(node);
                return base.Visit(node);
            }
            finally
            {
                currentErrors = errors;
                Registry = regBackup;
            }
        }

        protected override Expression VisitLiteralExpression(LiteralExpressionBindingParserNode node)
        {
            return Expression.Constant(node.Value);
        }

        protected override Expression VisitParenthesizedExpression(ParenthesizedExpressionBindingParserNode node)
        {
            // just visit content
            return Visit(node.InnerExpression);
        }

        protected override Expression VisitUnaryOperator(UnaryOperatorBindingParserNode node)
        {
            var operand = Visit(node.InnerExpression);
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
                default:
                    throw new NotSupportedException($"unary operator { node.Operator } is not supported");
            }
            return ExpressionHelper.GetUnaryOperator(operand, eop);
        }

        protected override Expression VisitBinaryOperator(BinaryOperatorBindingParserNode node)
        {
            var left = HandleErrors(node.FirstExpression, Visit);
            var right = HandleErrors(node.SecondExpression, Visit);
            ThrowOnErrors();

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
                case BindingTokenType.AssignOperator:
                    eop = ExpressionType.Assign;
                    break;
                default:
                    throw new NotSupportedException($"unary operator { node.Operator } is not supported");
            }

            return ExpressionHelper.GetBinaryOperator(left, right, eop);
        }

        protected override Expression VisitArrayAccess(ArrayAccessBindingParserNode node)
        {
            var target = HandleErrors(node.TargetExpression, Visit);
            var index = HandleErrors(node.ArrayIndexExpression, Visit);
            ThrowOnErrors();

            return ExpressionHelper.GetIndexer(target, index);
        }

        protected override Expression VisitFunctionCall(FunctionCallBindingParserNode node)
        {
            var target = HandleErrors(node.TargetExpression, Visit);
            var args = new Expression[node.ArgumentExpressions.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = HandleErrors(node.ArgumentExpressions[i], Visit);
            }
            ThrowOnErrors();

            return ExpressionHelper.Call(target, args);
        }

        protected override Expression VisitSimpleName(SimpleNameBindingParserNode node)
        {
            return GetMemberOrTypeExpression(node, null);
        }

        protected override Expression VisitConditionalExpression(ConditionalExpressionBindingParserNode node)
        {
            var condition = HandleErrors(node.ConditionExpression, n => TypeConversion.ImplicitConversion(Visit(n), typeof(bool), true));
            var trueExpr = HandleErrors(node.TrueExpression, Visit);
            var falseExpr = HandleErrors(node.FalseExpression, Visit);
            ThrowOnErrors();

            if (trueExpr.Type != falseExpr.Type)
            {
                trueExpr = TypeConversion.ImplicitConversion(trueExpr, falseExpr.Type, allowToString: true) ?? trueExpr;
                falseExpr = TypeConversion.ImplicitConversion(falseExpr, trueExpr.Type, allowToString: true) ?? falseExpr;
            }

            return Expression.Condition(condition, trueExpr, falseExpr);
        }

        protected override Expression VisitMemberAccess(MemberAccessBindingParserNode node)
        {
            var nameNode = node.MemberNameExpression;
            var typeParameters = nameNode is GenericNameBindingParserNode
                ? ResolveGenericArgumets(nameNode.CastTo<GenericNameBindingParserNode>())
                : null;
            var identifierName = (typeParameters?.Count() ?? 0) > 0
                ? $"{nameNode.Name}`{typeParameters.Count()}"
                : nameNode.Name;

            var target = Visit(node.TargetExpression);

            if (target is UnknownStaticClassIdentifierExpression)
            {
                var name = (target as UnknownStaticClassIdentifierExpression).Name + "." + identifierName;

                var resolvedTypeExpression = Registry.Resolve(name, throwOnNotFound: false) ?? new UnknownStaticClassIdentifierExpression(name);

                if (typeParameters != null)
                {
                    var resolvedType = resolvedTypeExpression.Type.MakeGenericType(typeParameters);
                    resolvedTypeExpression = new StaticClassIdentifierExpression(resolvedType);
                }
                return resolvedTypeExpression;
            }

            return ExpressionHelper.GetMember(target, nameNode.Name, typeParameters, onlyMemberTypes: ResolveOnlyTypeName);
        }

        protected override Expression VisitGenericName(GenericNameBindingParserNode node)
        {
            var typeParameters = ResolveGenericArgumets(node.CastTo<GenericNameBindingParserNode>());

            return GetMemberOrTypeExpression(node, typeParameters);
        }

        protected override Expression VisitLambda(LambdaBindingParserNode node)
        {
            // Create lambda definition
            var lambdaParameters = new ParameterExpression[node.ParameterExpressions.Count];
            for (var i = 0; i < lambdaParameters.Length; i++)
                lambdaParameters[i] = (ParameterExpression)HandleErrors(node.ParameterExpressions[i], Visit);

            // Make sure that parameter identifiers are distinct
            if (lambdaParameters.GroupBy(param => param.Name).Any(group => group.Count() > 1))
                throw new BindingCompilationException("Parameter identifiers must be unique.", node);

            // Make sure that parameter identifiers do not collide with existing symbols within registry
            var collision = lambdaParameters.FirstOrDefault(param => Registry.Resolve(param.Name, false) != null);
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
            return Expression.Lambda(body, lambdaParameters);
        }

        protected override Expression VisitLambdaParameter(LambdaParameterBindingParserNode node)
        {
            var parameterType = Visit(node.Type).Type;
            return Expression.Parameter(parameterType, node.Name.ToDisplayString());
        }

        protected override Expression VisitBlock(BlockBindingParserNode node)
        {
            var left = HandleErrors(node.FirstExpression, Visit) ?? Expression.Default(typeof(void));
            var right = HandleErrors(node.SecondExpression, Visit) ?? Expression.Default(typeof(void));
            ThrowOnErrors();

            if (typeof(Task).IsAssignableFrom(left.Type))
            {
                return ExpressionHelper.RewriteTaskSequence(left, right);
            }

            if (right is BlockExpression rightBlock)
            {
                // flat the `(a; b; c; d; e; ...)` expression down
                return Expression.Block(rightBlock.Variables, new Expression[] { left }.Concat(rightBlock.Expressions));
            }
            else return Expression.Block(left, right);
        }

        protected override Expression VisitVoid(VoidBindingParserNode node) => Expression.Default(typeof(void));

        private Expression GetMemberOrTypeExpression(IdentifierNameBindingParserNode node, Type[] typeParameters)
        {
            if (string.IsNullOrWhiteSpace(node.Name)) return null;

            var expr = 
                Scope == null 
                ? Registry.Resolve(node.Name, throwOnNotFound: false)
                : (ExpressionHelper.GetMember(Scope, node.Name, typeParameters, throwExceptions: false, onlyMemberTypes: ResolveOnlyTypeName)
                    ?? Registry.Resolve(node.Name, throwOnNotFound: false));

            if (expr == null) return new UnknownStaticClassIdentifierExpression(node.Name);
            if (expr is ParameterExpression && expr.Type == typeof(ExpressionHelper.UnknownTypeSentinel)) throw new Exception($"Type of '{expr}' could not be resolved.");
            return expr;
        }

        private Type[] ResolveGenericArgumets(GenericNameBindingParserNode node)
        {
            var parameters = new Type[node.TypeArguments.Count];

            for (int i = 0; i < node.TypeArguments.Count; i++)
            {
                var typeArgument = node.TypeArguments[i];

                parameters[i] = Visit(typeArgument).Type;
            }
            return parameters;
        }

        private void ThrowIfNotTypeNameRelevant(BindingParserNode node)
        {
            if (ResolveOnlyTypeName && !(node is MemberAccessBindingParserNode) && !(node is IdentifierNameBindingParserNode))
            {
                throw new Exception("Only type name is supported.");
            }
        }
    }
}
