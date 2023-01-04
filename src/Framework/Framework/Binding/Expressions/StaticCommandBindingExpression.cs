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
    /// <summary> The `{staticCommand: ...}` binding. It is a command, so should be used to handle events, but compared to `command`, it runs primarily client-side. Compared to `value` binding, `staticCommand`s are expected to have side effects and run asynchronously (the binding will return a Promise or a Task). </summary>
    [BindingCompilationRequirements(
        required: new[] { typeof(StaticCommandOptionsLambdaJavascriptProperty), /*typeof(BindingDelegate)*/ }
    )]
    [Options]
    public class StaticCommandBindingExpression : BindingExpression, IStaticCommandBinding
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }

        private protected MaybePropValue<StaticCommandOptionsLambdaJavascriptProperty> staticCommandLambdaJs;
        private protected MaybePropValue<ActionFiltersBindingProperty> actionFilters;

        private protected override void StoreProperty(object p)
        {
            if (p is StaticCommandOptionsLambdaJavascriptProperty staticCommandLambdaJs)
                this.staticCommandLambdaJs.SetValue(new(staticCommandLambdaJs));
            if (p is ActionFiltersBindingProperty actionFilters)
                this.actionFilters.SetValue(new(actionFilters));
            else
                base.StoreProperty(p);
        }

        public override object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (type == typeof(StaticCommandOptionsLambdaJavascriptProperty))
                return staticCommandLambdaJs.GetValue(this).GetValue(errorMode, this, type);
            if (type == typeof(ActionFiltersBindingProperty))
                return actionFilters.GetValue(this).GetValue(errorMode, this, type);
            return base.GetProperty(type, errorMode);
        }

        private protected override IEnumerable<object?> GetOutOfDictionaryProperties() =>
            base.GetOutOfDictionaryProperties().Concat(new object?[] {
                staticCommandLambdaJs.Value.Value,
                actionFilters.Value.Value,
            });


        public ImmutableArray<IActionFilter> ActionFilters => actionFilters.GetValueOrNull(this)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

        [Obsolete("StaticCommandBindingExpression.CommandJavascript is no longer supported. Use KnockoutHelper.GenerateClientPostBackExpression instead.")]
        public ParametrizedCode CommandJavascript =>
            this.GetProperty<StaticCommandJavascriptProperty>().Code;

        public ParametrizedCode OptionsLambdaJavascript => staticCommandLambdaJs.GetValueOrThrow(this).Code;

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {
                new Func<StaticCommandJsAstProperty, RequiredRuntimeResourcesBindingProperty>(js => {
                    var resources = js.Expression.DescendantNodesAndSelf().Select(n => n.Annotation<RequiredRuntimeResourcesBindingProperty>()).Where(n => n != null).SelectMany(n => n!.Resources).ToImmutableArray();
                    return resources.Length == 0 ? RequiredRuntimeResourcesBindingProperty.Empty : new RequiredRuntimeResourcesBindingProperty(resources);
                }),
                
                new Func<AssignedPropertyBindingProperty, ExpectedTypeBindingProperty>(property => {
                    var prop = property?.DotvvmProperty;
                    if (prop == null) return new ExpectedTypeBindingProperty(typeof(Command));

                    return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? (prop.PropertyType.GenericTypeArguments.SingleOrDefault() ?? typeof(Command)) : prop.PropertyType);
                })
            };
        }
    }

    public class StaticCommandBindingExpression<T>: StaticCommandBindingExpression, IStaticCommandBinding<T>
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}
