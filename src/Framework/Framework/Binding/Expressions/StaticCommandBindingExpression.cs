using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary> The `{staticCommand: ...}` binding. It is a command that runs primarily client-side and is well-suited to handle events. Compared to `value` binding, `staticCommand`s are expected to have side effects and run asynchronously (the binding will return a Promise or a Task). </summary>
    [BindingCompilationRequirements(
        required: new[] { typeof(StaticCommandOptionsLambdaJavascriptProperty), /*typeof(BindingDelegate)*/ }
    )]
    [Options]
    public class StaticCommandBindingExpression : BindingExpression, IStaticCommandBinding
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }

        private protected MaybePropValue<ParametrizedCode> staticCommandLambdaJs; // StaticCommandOptionsLambdaJavascriptProperty
        private protected MaybePropValue<ActionFiltersBindingProperty> actionFilters;

        private protected StaticCommandOptionsLambdaJavascriptProperty GetStaticCommandLambdaJs(out ErrorWrapper? error) =>
            staticCommandLambdaJs.GetValue(this).TryGet(out var value, out error)
                ? new StaticCommandOptionsLambdaJavascriptProperty(value!)
                : default;

        private protected override void StoreProperty(object p)
        {
            if (p is StaticCommandOptionsLambdaJavascriptProperty staticCommandLambdaJs)
                this.staticCommandLambdaJs.SetValue(new(staticCommandLambdaJs.Code, null));
            if (p is ActionFiltersBindingProperty actionFilters)
                this.actionFilters.SetValue(new(actionFilters, null));
            else
                base.StoreProperty(p);
        }

        private protected override bool TryGetPropertyVirtual(Type type, out PropValue<object> value)
        {
            if (type == typeof(ActionFiltersBindingProperty))
            {
                value = actionFilters.GetValue(this).AsObject();
                return true;
            }
            if (type == typeof(StaticCommandOptionsLambdaJavascriptProperty))
            {
                value = new(GetStaticCommandLambdaJs(out var error), error);
                return true;
            }
            return base.TryGetPropertyVirtual(type, out value);
        }

        private protected override bool TryGetPropertyVirtual<T>([MaybeNull] out T value, out ErrorWrapper? error)
        {
            if (typeof(T) == typeof(StaticCommandOptionsLambdaJavascriptProperty))
            {
                value = (T)(object)GetStaticCommandLambdaJs(out error);
                return true;
            }
            return base.TryGetPropertyVirtual<T>(out value, out error);
        }


        private protected override IEnumerable<object?> GetOutOfDictionaryProperties() =>
            base.GetOutOfDictionaryProperties().Concat(new object?[] {
                staticCommandLambdaJs.Value?.Apply(v => new StaticCommandOptionsLambdaJavascriptProperty(v)),
                actionFilters.Value,
            });


        public ImmutableArray<IActionFilter> ActionFilters => actionFilters.GetValueOrNull(this)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

        [Obsolete("StaticCommandBindingExpression.CommandJavascript is no longer supported. Use KnockoutHelper.GenerateClientPostBackExpression instead.")]
        public ParametrizedCode CommandJavascript =>
            this.GetProperty<StaticCommandJavascriptProperty>().Code;

        public ParametrizedCode OptionsLambdaJavascript => staticCommandLambdaJs.GetValueOrThrow(this);

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => [
                ..BindingCompilationService.GetDelegates([ new CommandBindingExpression.OptionsAttribute.CommonCommandMethods() ]),
                new Func<StaticCommandJsAstProperty, RequiredRuntimeResourcesBindingProperty>(js => {
                    var resources = js.Expression.DescendantNodesAndSelf().Select(n => n.Annotation<RequiredRuntimeResourcesBindingProperty>()).Where(n => n != null).SelectMany(n => n!.Resources).ToImmutableArray();
                    return resources.Length == 0 ? RequiredRuntimeResourcesBindingProperty.Empty : new RequiredRuntimeResourcesBindingProperty(resources);
                }),

                new Func<AssignedPropertyBindingProperty, ExpectedTypeBindingProperty>(property => {
                    var prop = property.DotvvmProperty;
                    if (prop == null) return new ExpectedTypeBindingProperty(typeof(Command));

                    return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? (prop.PropertyType.GenericTypeArguments.SingleOrDefault() ?? typeof(Command)) : prop.PropertyType);
                })
            ];
        }
    }

    public class StaticCommandBindingExpression<T> : StaticCommandBindingExpression, IStaticCommandBinding<T>
    {
        public StaticCommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}
