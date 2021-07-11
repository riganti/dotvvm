using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(
        required: new[] { typeof(StaticCommandJavascriptProperty), /*typeof(BindingDelegate)*/ }
    )]
    [Options]
    public class StaticCommandBindingExpression : BindingExpression, IStaticCommandBinding
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public ImmutableArray<IActionFilter> ActionFilters => this.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public BindingDelegate BindingDelegate => this.GetProperty<BindingDelegate>();

        public ParametrizedCode CommandJavascript => this.GetProperty<StaticCommandJavascriptProperty>().Code;

        public ParametrizedCode OptionsLambdaJavascript => this.GetProperty<StaticCommandOptionsLambdaJavascriptProperty>().Code;

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() =>
                BindingCompilationService.GetDelegates(new object[] { new StaticCommandMethods(), new CommandBindingExpression.CommonCommandResolverMethods() });
        }

        public class StaticCommandMethods
        {
            public RequiredRuntimeResourcesBindingProperty GetRequiredResources(StaticCommandJsAstProperty js)
            {
                var resources = js.Expression.DescendantNodesAndSelf().Select(n => n.Annotation<RequiredRuntimeResourcesBindingProperty>()).Where(n => n != null).SelectMany(n => n.Resources).ToImmutableArray();
                return resources.Length == 0 ? RequiredRuntimeResourcesBindingProperty.Empty : new RequiredRuntimeResourcesBindingProperty(resources);
            }
        }
    }

    public class StaticCommandBindingExpression<T>: StaticCommandBindingExpression, IStaticCommandBinding<T>
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}
