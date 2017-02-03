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

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingPropertyResolvers
    {
        private readonly DotvvmConfiguration configuration;
        private readonly IBindingExpressionBuilder bindingParser;

        public BindingPropertyResolvers(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.bindingParser = configuration.ServiceLocator.GetService<IBindingExpressionBuilder>();
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
            return new KnockoutJsExpressionBindingProperty(JavascriptTranslator.RemoveTopObservables(
                   JavascriptTranslator.CompileToJavascript(expression.Expression, dataContext, configuration.ServiceLocator.GetService<IViewModelSerializationMapper>())));
        }

        public KnockoutExpressionBindingProperty FormatJavascript(KnockoutJsExpressionBindingProperty expression)
        {
            return new KnockoutExpressionBindingProperty(expression.Expression.FormatParametrizedScript(niceMode: configuration.Debug));
        }

        public ResultTypeBindingProperty GetResultType(ParsedExpressionBindingProperty expression) => new ResultTypeBindingProperty(expression.Expression.Type);

        public ExpectedTypeBindingProperty GetExpectedType(AssignedPropertyBindingProperty property = null)
        {
            var prop = property?.DotvvmProperty;
            if (prop == null) return new ExpectedTypeBindingProperty(typeof(object));

            return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? typeof(object) : prop.PropertyType);
        }

        public BindingAdditionalResolvers GetAdditionalResolversFromProperty(AssignedPropertyBindingProperty property = null, DataContextStack stack = null)
        {
            var prop = property?.DotvvmProperty;
            if (prop == null) return new BindingAdditionalResolvers(Enumerable.Empty<Delegate>());

            return new BindingAdditionalResolvers(
                (prop.PropertyInfo?.GetCustomAttributes<BindingCompilationOptionsAttribute>() ?? Enumerable.Empty<BindingCompilationOptionsAttribute>())
                .SelectMany(o => o.GetResolvers())
                .ToImmutableArray());
        }

        public DataContextSpaceIdBindingProperty GetSpaceId(DataContextStack context) => new DataContextSpaceIdBindingProperty(context.DataContextSpaceId);

        public CompiledBindingExpression.BindingDelegate Compile(Expression<CompiledBindingExpression.BindingDelegate> expr) => expr.Compile();
        public CompiledBindingExpression.BindingUpdateDelegate Compile(Expression<CompiledBindingExpression.BindingUpdateDelegate> expr) => expr.Compile();

        public IdBindingProperty GetIdFromOriginalString(OriginalStringBindingProperty binding) => new IdBindingProperty(binding.Code);

        public DataSourceAccessBinding GetDataSourceAccess(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            if (typeof(IGridViewDataSet).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceAccessBinding(binding.DeriveBinding(new ParsedExpressionBindingProperty(
                    Expression.Property(expression.Expression, nameof(IGridViewDataSet.Items))
                )));
            else if (typeof(IEnumerable).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceAccessBinding(binding);
            else throw new NotSupportedException($"Can not make datasource from binding '{expression.Expression}'.");
        }

        public DataSourceLengthBinding GetDataSourceLength(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            if (expression.Expression.Type.GetInterfaces().First(i => i == typeof(ICollection) || i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)) is Type ifc)
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Property(expression.Expression, ifc.GetProperty(nameof(ICollection.Count)))
                    )));
            else if (typeof(IGridViewDataSet).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceLengthBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(
                        Expression.Property(Expression.Property(expression.Expression, nameof(IGridViewDataSet.Items)), typeof(ICollection).GetProperty(nameof(ICollection.Count)))
                    )));
            else throw new NotSupportedException($"Can not find collection length from binding '{expression.Expression}'.");
        }

        public DataSourceCurrentElementBinding GetDataSourceCurrentElement(ParsedExpressionBindingProperty expression, IBinding binding)
        {
            Expression makeIndexer(Expression expr) =>
                expr.Type.GetProperty("Item") is PropertyInfo indexer && indexer.GetMethod?.GetParameters()?.Length == 1 ?
                Expression.MakeIndex(expr, indexer, new[] {
                    Expression.Parameter(typeof(int), "_index").AddParameterAnnotation(
                        new BindingParameterAnnotation(extensionParameter: new CurrentCollectionIndexExtensionParameter()))
                }) :
                null;
            
            if (makeIndexer(expression.Expression) is Expression r)
                return new DataSourceCurrentElementBinding(binding.DeriveBinding(new ParsedExpressionBindingProperty(r)));

            else if (typeof(IGridViewDataSet).IsAssignableFrom(expression.Expression.Type))
                return new DataSourceCurrentElementBinding(binding.DeriveBinding(
                    new ParsedExpressionBindingProperty(makeIndexer(Expression.Property(expression.Expression, nameof(IGridViewDataSet.Items))))));

            else throw new NotSupportedException($"Can not access current element on binding '{expression.Expression}'.");

        }
        //public OriginalStringBindingProperty
    }
}
