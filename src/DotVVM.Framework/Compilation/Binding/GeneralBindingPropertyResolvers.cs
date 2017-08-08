using System;
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
using DotVVM.Framework.ViewModel.Serialization;
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

        public BindingPropertyResolvers(IBindingExpressionBuilder bindingParser, StaticCommandBindingCompiler staticCommandBindingCompiler, JavascriptTranslator javascriptTranslator, DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.bindingParser = bindingParser;
            this.staticCommandBindingCompiler = staticCommandBindingCompiler;
            this.javascriptTranslator = javascriptTranslator;
        }

        public ActionFiltersBindingProperty GetActionFilters(ParsedExpressionBindingProperty parsedExpression)
        {
            var list = new List<IActionFilter>();
            parsedExpression.Expression.ForEachMember(m => {
                list.AddRange(ReflectionUtils.GetCustomAttributes<IActionFilter>(m));
            });
            return new ActionFiltersBindingProperty(list.ToImmutableArray());
        }

        public Expression<CompiledBindingExpression.BindingDelegate> CompileToDelegate(
            CastedExpressionBindingProperty expression, DataContextStack dataContext)
        {
            var expr = BindingCompiler.ReplaceParameters(expression.Expression, dataContext);
            expr = new ExpressionNullPropagationVisitor(e => true).Visit(expr);
            expr = ExpressionUtils.ConvertToObject(expr);
            return Expression.Lambda<CompiledBindingExpression.BindingDelegate>(expr, BindingCompiler.ViewModelsParameter, BindingCompiler.CurrentControlParameter);
        }

        public CastedExpressionBindingProperty ConvertExpressionToType(ParsedExpressionBindingProperty expr, ExpectedTypeBindingProperty expectedType = null) =>
            new CastedExpressionBindingProperty(TypeConversion.ImplicitConversion(expr.Expression, expectedType?.Type ?? typeof(object), throwException: true, allowToString: true));

        public Expression<CompiledBindingExpression.BindingUpdateDelegate> CompileToUpdateDelegate(ParsedExpressionBindingProperty binding, DataContextStack dataContext)
        {
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var expr = BindingCompiler.ReplaceParameters(binding.Expression, dataContext);

            // don't throw exception, it is annoying to debug.
            if (expr.NodeType != ExpressionType.Parameter &&
                (expr.NodeType != ExpressionType.MemberAccess || (!expr.CastTo<MemberExpression>().Member.As<PropertyInfo>()?.CanWrite ?? false)) &&
                expr.NodeType != ExpressionType.Index) return null;

            var assignment = Expression.Assign(expr, Expression.Convert(valueParameter, expr.Type));
            return Expression.Lambda<CompiledBindingExpression.BindingUpdateDelegate>(assignment, BindingCompiler.ViewModelsParameter, BindingCompiler.CurrentControlParameter, valueParameter);
        }

        public BindingParserOptions GetDefaultBindingParserOptions(IBinding binding)
        {
            return new BindingParserOptions(binding.GetType());
        }

        public ParsedExpressionBindingProperty GetExpression(OriginalStringBindingProperty originalString, DataContextStack dataContext, BindingParserOptions options)
        {
            var expr = bindingParser.Parse(originalString.Code, dataContext, options);
            return new ParsedExpressionBindingProperty(expr.Reduce());
        }

        public KnockoutJsExpressionBindingProperty CompileToJavascript(ParsedExpressionBindingProperty expression,
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
            return new KnockoutExpressionBindingProperty(FormatJavascript(expression.Expression, true, niceMode: configuration.Debug), FormatJavascript(expression.Expression, false, niceMode: configuration.Debug));
        }

        public static ParametrizedCode FormatJavascript(JsExpression node, bool allowObservableResult = true, bool niceMode = false, bool nullChecks = true)
        {
            var expr = new JsParenthesizedExpression(node.Clone());
            expr.AcceptVisitor(new KnockoutObservableHandlingVisitor(allowObservableResult));
            if (nullChecks) JavascriptNullCheckAdder.AddNullChecks(expr);
            expr = new JsParenthesizedExpression((JsExpression)JsTemporaryVariableResolver.ResolveVariables(expr.Expression.Detach()));
            return (StartsWithStatementLikeExpression(expr.Expression) ? expr : expr.Expression).FormatParametrizedScript(niceMode);
        }

        public RequiredRuntimeResourcesBindingProperty GetRequiredResources(KnockoutJsExpressionBindingProperty js)
        {
            var resources = js.Expression.DescendantNodesAndSelf().Select(n => n.Annotation<RequiredRuntimeResourcesBindingProperty>()).Where(n => n != null).SelectMany(n => n.Resources).ToImmutableArray();
            return resources.Length == 0 ? RequiredRuntimeResourcesBindingProperty.Empty : new RequiredRuntimeResourcesBindingProperty(resources);
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
                (prop.PropertyInfo?.GetCustomAttributes<BindingCompilationRequirementsAttribute>() ?? Enumerable.Empty<BindingCompilationRequirementsAttribute>())
                .Aggregate((a, b) => a.ApplySecond(b));
        }

        public CompiledBindingExpression.BindingDelegate Compile(Expression<CompiledBindingExpression.BindingDelegate> expr) => expr.Compile();
        public CompiledBindingExpression.BindingUpdateDelegate Compile(Expression<CompiledBindingExpression.BindingUpdateDelegate> expr) => expr.Compile();


        private ConditionalWeakTable<ResolvedTreeRoot, ConcurrentDictionary<DataContextStack, int>> bindingCounts = new ConditionalWeakTable<ResolvedTreeRoot, ConcurrentDictionary<DataContextStack, int>>();

        public IdBindingProperty CreateBindingId(
            OriginalStringBindingProperty originalString = null,
            ParsedExpressionBindingProperty expression = null,
            DataContextStack dataContext = null,
            ResolvedBinding resolvedBinding = null,
            AssignedPropertyBindingProperty assignedProperty = null)
        {
            var sb = new StringBuilder();

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
            sb.Append(assignedProperty?.DotvvmProperty?.FullName);

            if (resolvedBinding?.TreeRoot != null)
            {
                var bindingIndex = bindingCounts.GetOrCreateValue(resolvedBinding.TreeRoot).AddOrUpdate(dataContext, 0, (_, i) => i + 1);
                sb.Append(" || ");
                sb.Append(bindingIndex);
            }

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString()));
                // use just 12 bytes = 96 bits
                return new IdBindingProperty(Convert.ToBase64String(hash, 0, 12));
            }
        }

        public DataSourceAccessBinding GetDataSourceAccess(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            if (typeof(IBaseGridViewDataSet).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceAccessBinding(binding.DeriveBinding(new ParsedExpressionBindingProperty(
                    Expression.Property(expression.Expression, nameof(IBaseGridViewDataSet.Items))
                )));
            else if (typeof(IEnumerable).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceAccessBinding(binding);
            else throw new NotSupportedException($"Can not make datasource from binding '{expression.Expression}'.");
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

        public StaticCommandJavascriptProperty CompileStaticCommand(DataContextStack dataContext, ParsedExpressionBindingProperty expression)
        {
            return new StaticCommandJavascriptProperty(FormatJavascript(this.staticCommandBindingCompiler.CompileToJavascript(dataContext, expression.Expression), niceMode: configuration.Debug));
        }

        public LocationInfoBindingProperty GetLocationInfo(ResolvedBinding resolvedBinding)
        {
            if (resolvedBinding.Parent == null) throw new Exception();
            return new LocationInfoBindingProperty(
                resolvedBinding.TreeRoot.FileName,
                resolvedBinding.DothtmlNode?.Tokens?.Select(t => (t.StartPosition, t.EndPosition)).ToArray(),
                resolvedBinding.DothtmlNode?.Tokens?.FirstOrDefault()?.LineNumber ?? -1,
                resolvedBinding.TreeRoot.GetAncestors().OfType<ResolvedControl>().FirstOrDefault()?.Metadata?.Type);
        }

        public SelectorItemBindingProperty GetItemLambda(ParsedExpressionBindingProperty expression, DataContextStack dataContext, IValueBinding binding)
        {
            var argument = Expression.Parameter(dataContext.DataContextType, "i");
            return new SelectorItemBindingProperty(binding.DeriveBinding(
                dataContext.Parent,
                Expression.Lambda(expression.Expression.ReplaceAll(e =>
                    e.GetParameterAnnotation() is BindingParameterAnnotation annotation &&
                        annotation.DataContext == dataContext &&
                        annotation.ExtensionParameter == null ?
                   argument :
                   e), argument)
            ));
        }
    }
}
