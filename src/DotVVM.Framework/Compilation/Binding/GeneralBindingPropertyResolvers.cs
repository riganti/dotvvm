﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingPropertyResolvers
    {
        private readonly DotvvmConfiguration configuration;
        private readonly IBindingExpressionBuilder bindingParser;
        private readonly StaticCommandBindingCompiler staticCommandBindingCompiler;
        private readonly JavascriptTranslator javascriptTranslator;
        private readonly MemberExpressionFactory memberExpressionFactory;

        public BindingPropertyResolvers(IBindingExpressionBuilder bindingParser, StaticCommandBindingCompiler staticCommandBindingCompiler, JavascriptTranslator javascriptTranslator, DotvvmConfiguration configuration, MemberExpressionFactory memberExpressionFactory)
        {
            this.configuration = configuration;
            this.bindingParser = bindingParser;
            this.staticCommandBindingCompiler = staticCommandBindingCompiler;
            this.javascriptTranslator = javascriptTranslator;
            this.memberExpressionFactory = memberExpressionFactory;
        }

        public ActionFiltersBindingProperty GetActionFilters(ParsedExpressionBindingProperty parsedExpression)
        {
            var list = new List<IActionFilter>();
            parsedExpression.Expression.ForEachMember(m => {
                list.AddRange(ReflectionUtils.GetCustomAttributes<IActionFilter>(m));
            });
            return new ActionFiltersBindingProperty(list.ToImmutableArray());
        }

        public Expression<BindingDelegate> CompileToDelegate(
            CastedExpressionBindingProperty expression, DataContextStack dataContext)
        {
            var expr = BindingCompiler.ReplaceParameters(expression.Expression, dataContext);
            expr = new ExpressionNullPropagationVisitor(e => true).Visit(expr);
            expr = ExpressionUtils.ConvertToObject(expr);
            return Expression.Lambda<BindingDelegate>(expr, BindingCompiler.ViewModelsParameter, BindingCompiler.CurrentControlParameter);
        }

        public CastedExpressionBindingProperty ConvertExpressionToType(ParsedExpressionBindingProperty expr, ExpectedTypeBindingProperty expectedType = null)
        {
            var destType = expectedType?.Type ?? typeof(object);
            var convertedExpr = TypeConversion.ImplicitConversion(expr.Expression, destType, throwException: false, allowToString: true);
            return new CastedExpressionBindingProperty(
                // if the expression is of type object (i.e. null literal) try the lambda conversion.
                convertedExpr != null && expr.Expression.Type != typeof(object) ? convertedExpr :
                TypeConversion.MagicLambdaConversion(expr.Expression, destType) ??
                TypeConversion.ImplicitConversion(expr.Expression, destType, throwException: true, allowToString: true)
            );
        }

        public Expression<BindingUpdateDelegate> CompileToUpdateDelegate(ParsedExpressionBindingProperty binding, DataContextStack dataContext)
        {
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var body = BindingCompiler.ReplaceParameters(binding.Expression, dataContext);
            body = memberExpressionFactory.UpdateMember(body, valueParameter);
            if (body == null)
            {
                return null;
            }

            return Expression.Lambda<BindingUpdateDelegate>(
                body,
                BindingCompiler.ViewModelsParameter,
                BindingCompiler.CurrentControlParameter,
                valueParameter);
        }

        public BindingParserOptions GetDefaultBindingParserOptions(IBinding binding)
        {
            return new BindingParserOptions(binding.GetType());
        }

        public ParsedExpressionBindingProperty GetExpression(OriginalStringBindingProperty originalString, DataContextStack dataContext, BindingParserOptions options, ExpectedTypeBindingProperty expectedType = null)
        {
            var expr = bindingParser.ParseWithLambdaConversion(originalString.Code, dataContext, options, expectedType?.Type ?? typeof(object));
            if (expr is StaticClassIdentifierExpression)
                throw new Exception($"'{originalString.Code}' is a static class reference, not a valid expression.");
            else if (expr is UnknownStaticClassIdentifierExpression)
                expr = expr.Reduce();
            return new ParsedExpressionBindingProperty(expr);
        }

        public KnockoutJsExpressionBindingProperty CompileToJavascript(CastedExpressionBindingProperty expression,
            DataContextStack dataContext)
        {
            return new KnockoutJsExpressionBindingProperty(
                   javascriptTranslator.CompileToJavascript(expression.Expression, dataContext).ApplyAction(a => a.Freeze()));
        }

        public SimplePathExpressionBindingProperty FormatSimplePath(KnockoutJsExpressionBindingProperty expression)
        {
            // if contains api parameter, can't use this as a path
            if (expression.Expression.DescendantNodes().Any(n => n.TryGetAnnotation(out ViewModelInfoAnnotation vmInfo) && vmInfo.ExtensionParameter is RestApiRegistrationHelpers.ApiExtensionParameter apiParameter))
                throw new Exception($"Can't get a path expression for command binding from binding that is using rest api.");
            return new SimplePathExpressionBindingProperty(expression.Expression.FormatParametrizedScript());
        }

        public KnockoutExpressionBindingProperty FormatJavascript(KnockoutJsExpressionBindingProperty expression)
        {
            return new KnockoutExpressionBindingProperty(
                FormatJavascript(expression.Expression, true, niceMode: configuration.Debug),
                FormatJavascript(expression.Expression, false, niceMode: configuration.Debug),
                FormatJavascript(expression.Expression.Clone().EnsureObservableWrapped(), true, niceMode: configuration.Debug));
        }

        public static ParametrizedCode FormatJavascript(JsExpression node, bool allowObservableResult = true, bool niceMode = false, bool nullChecks = true)
        {
            var expr = new JsParenthesizedExpression(node.Clone());
            expr.AcceptVisitor(new KnockoutObservableHandlingVisitor(allowObservableResult));
            if (nullChecks) JavascriptNullCheckAdder.AddNullChecks(expr);
            expr = new JsParenthesizedExpression((JsExpression)JsTemporaryVariableResolver.ResolveVariables(expr.Expression.Detach()));
            return (StartsWithStatementLikeExpression(expr.Expression) ? expr : expr.Expression).FormatParametrizedScript(niceMode);
        }

        private static bool StartsWithStatementLikeExpression(JsExpression expression)
        {
            if (expression is JsFunctionExpression || expression is JsObjectExpression) return true;
            if (expression == null || !expression.HasChildren ||
                expression is JsParenthesizedExpression ||
                expression is JsUnaryExpression unary && unary.IsPrefix ||
                expression is JsNewExpression ||
                expression is JsArrayExpression) return false;
            return StartsWithStatementLikeExpression(expression.FirstChild as JsExpression);
        }

        public ResultTypeBindingProperty GetResultType(ParsedExpressionBindingProperty expression) => new ResultTypeBindingProperty(expression.Expression.Type);

        public ExpectedTypeBindingProperty GetExpectedType(AssignedPropertyBindingProperty property = null)
        {
            var prop = property?.DotvvmProperty;
            if (prop == null) return new ExpectedTypeBindingProperty(typeof(object));

            return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? (prop.PropertyType.GenericTypeArguments.SingleOrDefault() ?? typeof(object)) : prop.PropertyType);
        }

        public BindingResolverCollection GetAdditionalResolversFromProperty(AssignedPropertyBindingProperty property = null, DataContextStack stack = null)
        {
            var prop = property?.DotvvmProperty;
            if (prop == null) return new BindingResolverCollection(Enumerable.Empty<Delegate>());

            return new BindingResolverCollection(
                (prop.PropertyInfo?.GetCustomAttributes<BindingCompilationOptionsAttribute>() ?? Enumerable.Empty<BindingCompilationOptionsAttribute>())
                .SelectMany(o => o.GetResolvers())
                .Concat(stack.EnumerableItems().Reverse().SelectMany(s => s.BindingPropertyResolvers))
                .ToImmutableArray());
        }

        public BindingCompilationRequirementsAttribute GetAdditionalResolversFromProperty(AssignedPropertyBindingProperty property)
        {
            var prop = property?.DotvvmProperty;
            if (prop == null) return new BindingCompilationRequirementsAttribute();

            return
                new[] { new BindingCompilationRequirementsAttribute() }
                .Concat(prop.PropertyInfo?.GetCustomAttributes<BindingCompilationRequirementsAttribute>() ?? Enumerable.Empty<BindingCompilationRequirementsAttribute>())
                .Aggregate((a, b) => a.ApplySecond(b));
        }


        private ConditionalWeakTable<ResolvedTreeRoot, ConcurrentDictionary<DataContextStack, int>> bindingCounts = new ConditionalWeakTable<ResolvedTreeRoot, ConcurrentDictionary<DataContextStack, int>>();

        public IdBindingProperty CreateBindingId(
            OriginalStringBindingProperty originalString = null,
            ParsedExpressionBindingProperty expression = null,
            DataContextStack dataContext = null,
            ResolvedBinding resolvedBinding = null,
            LocationInfoBindingProperty locationInfo = null)
        {
            var sb = new StringBuilder();

            if (resolvedBinding?.TreeRoot != null && dataContext != null)
            {
                var bindingIndex = bindingCounts.GetOrCreateValue(resolvedBinding.TreeRoot).AddOrUpdate(dataContext, 0, (_, i) => i + 1);
                sb.Append(bindingIndex);
                sb.Append(" || ");
            }

            // don't append expression when original string is present, so it does not have to be always exactly same
            if (originalString != null)
                sb.Append(originalString.Code);
            else sb.Append(expression.Expression.ToString());

            sb.Append(" || ");
            while (dataContext != null)
            {
                sb.Append(dataContext.DataContextType.FullName);
                sb.Append('(');
                foreach (var ns in dataContext.NamespaceImports)
                {
                    sb.Append(ns.Alias);
                    sb.Append('=');
                    sb.Append(ns.Namespace);
                }
                sb.Append(';');
                foreach (var ext in dataContext.ExtensionParameters)
                {
                    sb.Append(ext.Identifier);
                    if (ext.Inherit) sb.Append('*');
                    sb.Append(':');
                    sb.Append(ext.ParameterType.FullName);
                    sb.Append(':');
                    sb.Append(ext.GetType().FullName);
                }
                sb.Append(") -- ");
                dataContext = dataContext.Parent;
            }
            sb.Append(" || ");
            sb.Append(locationInfo?.RelatedProperty?.FullName);

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString()));
                // use just 12 bytes = 96 bits
                return new IdBindingProperty(Convert.ToBase64String(hash, 0, 12));
            }
        }

        public NegatedBindingExpression NegateBinding(ParsedExpressionBindingProperty e, IBinding binding)
        {
            return new NegatedBindingExpression(binding.DeriveBinding(
                new ParsedExpressionBindingProperty(
                    // Not, Equals and NotEquals are safe to optimize for both .NET and Javascript (if that the negated value was already a boolean)
                    // but comparison operators are not safe to optimize as `null > 0` and `null <= 0` are both true on .NET (not JS, so it's possible to optimize this in the JsAST)
                    // On the other hand it would not be possible to optimize Not(Not(...)) in the JsAST, because you can't be so sure about the type of the expression
                    e.Expression.NodeType == ExpressionType.Not ? e.Expression.CastTo<UnaryExpression>().Operand :
                    e.Expression.NodeType == ExpressionType.Equal ? e.Expression.CastTo<BinaryExpression>().UpdateType(ExpressionType.NotEqual) :
                    e.Expression.NodeType == ExpressionType.NotEqual ? e.Expression.CastTo<BinaryExpression>().UpdateType(ExpressionType.Equal) :
                    (Expression)Expression.Not(e.Expression)
                )
            ));
        }
        public ExpectedAsStringBindingExpression ExpectAsStringBinding(ParsedExpressionBindingProperty e, IBinding binding)
        {
            return new ExpectedAsStringBindingExpression(binding.DeriveBinding(new ExpectedTypeBindingProperty(typeof(string)), e));
        }


        public DataSourceAccessBinding GetDataSourceAccess(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            if (typeof(IBaseGridViewDataSet).IsAssignableFrom(expression.Expression.Type) && !expression.Expression.Type.IsInterface)
                return new DataSourceAccessBinding(binding.DeriveBinding(new ParsedExpressionBindingProperty(
                    Expression.Property(expression.Expression, nameof(IBaseGridViewDataSet.Items))
                )));
            else if (typeof(IEnumerable).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceAccessBinding(binding);
            else throw new NotSupportedException($"Can not make datasource from binding '{expression.Expression}' of type '{expression.Expression.Type}'.");
        }

        public DataSourceLengthBinding GetDataSourceLength(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            if (expression.Expression.Type.Implements(typeof(ICollection), out var ifc) || expression.Expression.Type.Implements(typeof(ICollection<>), out ifc))
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Property(expression.Expression, ifc.GetProperty(nameof(ICollection.Count)))
                    )));
            else if (expression.Expression.Type.Implements(typeof(IBaseGridViewDataSet), out var igridviewdataset))
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Property(Expression.Property(expression.Expression, igridviewdataset.GetProperty(nameof(IBaseGridViewDataSet.Items))), typeof(ICollection).GetProperty(nameof(ICollection.Count)))
                    )));
            else if (expression.Expression.Type == typeof(string))
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Property(expression.Expression, nameof(String.Length))
                    )));
            else if (expression.Expression.Type.Implements(typeof(IEnumerable<>)))
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Call(typeof(Enumerable), "Count", new[] { ReflectionUtils.GetEnumerableType(expression.Expression.Type) }, expression.Expression)
                    )));
            else throw new NotSupportedException($"Can not find collection length from binding '{expression.Expression}'.");
        }

        public DataSourceCurrentElementBinding GetDataSourceCurrentElement(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            Expression indexParameter() => Expression.Parameter(typeof(int), "_index").AddParameterAnnotation(
                new BindingParameterAnnotation(extensionParameter: new CurrentCollectionIndexExtensionParameter()));
            Expression makeIndexer(Expression expr) =>
                expr.Type.GetProperty("Item") is PropertyInfo indexer && indexer.GetMethod?.GetParameters()?.Length == 1 ?
                    Expression.MakeIndex(expr, indexer, new[] { indexParameter() }) :
                expr.Type.IsArray ?
                    Expression.ArrayIndex(expr, indexParameter()) :
                expression.Expression.Type.Implements(typeof(IEnumerable<>), out var ienumerable) ?
                    (Expression)Expression.Call(typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(ienumerable.GetGenericArguments()), expression.Expression, indexParameter()) :
                null;

            if (makeIndexer(expression.Expression) is Expression r)
                return new DataSourceCurrentElementBinding(binding.DeriveBinding(new ParsedExpressionBindingProperty(r)));

            else if (typeof(IBaseGridViewDataSet).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceCurrentElementBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(makeIndexer(Expression.Property(expression.Expression, nameof(IBaseGridViewDataSet.Items))))));
            else throw new NotSupportedException($"Can not access current element on binding '{expression.Expression}' of type '{expression.Expression.Type}'.");
        }


        public StaticCommandJsAstProperty CompileStaticCommand(DataContextStack dataContext, ParsedExpressionBindingProperty expression) =>
            new StaticCommandJsAstProperty(this.staticCommandBindingCompiler.CompileToJavascript(dataContext, expression.Expression));

        public StaticCommandJavascriptProperty FormatStaticCommand(StaticCommandJsAstProperty code) =>
            new StaticCommandJavascriptProperty(FormatJavascript(code.Expression, niceMode: configuration.Debug));

        public LocationInfoBindingProperty GetLocationInfo(ResolvedBinding resolvedBinding, AssignedPropertyBindingProperty assignedProperty = null)
        {
            return new LocationInfoBindingProperty(
                resolvedBinding.TreeRoot?.FileName,
                resolvedBinding.DothtmlNode?.Tokens?.Select(t => (t.StartPosition, t.EndPosition)).ToArray(),
                resolvedBinding.DothtmlNode?.Tokens?.FirstOrDefault()?.LineNumber ?? -1,
                resolvedBinding.GetAncestors().OfType<ResolvedControl>().FirstOrDefault()?.Metadata?.Type,
                assignedProperty?.DotvvmProperty
            );
        }

        public SelectorItemBindingProperty GetItemLambda(ParsedExpressionBindingProperty expression, DataContextStack dataContext, IValueBinding binding)
        {
            var argument = Expression.Parameter(dataContext.DataContextType, "i");
            return new SelectorItemBindingProperty(binding.DeriveBinding(
                dataContext.Parent,
                Expression.Lambda(expression.Expression.ReplaceAll(e =>
                        e?.GetParameterAnnotation() is BindingParameterAnnotation annotation &&
                        annotation.DataContext == dataContext &&
                        annotation.ExtensionParameter == null ?
                   argument :
                   e), argument)
            ));
        }

        public ThisBindingProperty GetThisBinding(IBinding binding, DataContextStack stack)
        {
            var thisBinding = binding.DeriveBinding(
                new ParsedExpressionBindingProperty(Expression.Parameter(stack.DataContextType, "_this").AddParameterAnnotation(new BindingParameterAnnotation(stack)))
            );

            return new ThisBindingProperty(thisBinding);
        }

        public CollectionElementDataContextBindingProperty GetCollectionElementDataContext(DataContextStack dataContext, ResultTypeBindingProperty resultType)
        {
            return new CollectionElementDataContextBindingProperty(DataContextStack.Create(
                ReflectionUtils.GetEnumerableType(resultType.Type),
                parent: dataContext,
                extensionParameters: new CollectionElementDataContextChangeAttribute(0).GetExtensionParameters(new ResolvedTypeDescriptor(dataContext.DataContextType)).ToArray()
            ));
        }

        public IsMoreThanZeroBindingProperty IsMoreThanZero(ParsedExpressionBindingProperty expr, IBinding binding)
        {
            return new IsMoreThanZeroBindingProperty(binding.DeriveBinding(
                new ParsedExpressionBindingProperty(Expression.GreaterThan(expr.Expression, Expression.Constant(0)))
            ));
        }
    }
}
