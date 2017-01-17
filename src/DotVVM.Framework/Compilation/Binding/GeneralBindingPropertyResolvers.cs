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
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmBindableObject), "controlRoot");
            var expr = ExpressionUtils.Replace(expression.Expression, BindingCompiler.GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            expr = ExpressionUtils.ConvertToObject(expr);
            return Expression.Lambda<CompiledBindingExpression.BindingDelegate>(expr, viewModelsParameter, controlRootParameter);
        }

        public CastedExpressionBindingProperty ConvertExpressionToType(ParsedExpressionBindingProperty expr, ExpectedTypeBindingProperty expectedType = null)
            => new CastedExpressionBindingProperty(TypeConversion.ImplicitConversion(expr.Expression, expectedType?.Type ?? typeof(object), throwException: true, allowToString: true));

        public Expression<CompiledBindingExpression.BindingUpdateDelegate> CompileToUpdateDelegate(ParsedExpressionBindingProperty binding, DataContextStack dataContext)
        {
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmBindableObject), "controlRoot");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var expr = ExpressionUtils.Replace(binding.Expression, BindingCompiler.GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            var assignment = Expression.Assign(expr, Expression.Convert(valueParameter, expr.Type));
            return Expression.Lambda<CompiledBindingExpression.BindingUpdateDelegate>(assignment, viewModelsParameter, controlRootParameter, valueParameter);
        }

        public ParsedExpressionBindingProperty GetExpression(OriginalStringBindingProperty originalString, DataContextStack dataContext, BindingParserOptions options)
        {
            var expr = bindingParser.Parse(originalString.Code, dataContext, options);
            return new ParsedExpressionBindingProperty(expr);
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

        public BindingAdditionalResolvers GetAdditionalResolversFromProperty(AssignedPropertyBindingProperty property = null)
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

        //public OriginalStringBindingProperty
    }
}
