using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(
        required: new[] { typeof(StaticCommandJavascriptProperty), typeof(CompiledBindingExpression.BindingDelegate) }
    )]
    [Options]
    public class StaticCommandBindingExpression : BindingExpression, ICommandBinding
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public ImmutableArray<IActionFilter> ActionFilters => this.GetProperty<ActionFiltersBindingProperty>(optional: true)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public CompiledBindingExpression.BindingDelegate BindingDelegate => this.GetProperty<CompiledBindingExpression.BindingDelegate>();

        public ParametrizedCode CommandJavascript => this.GetProperty<StaticCommandJavascriptProperty>().Code;


        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
        }
    }
}
