#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using DotVVM.Framework.Configuration;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleMatcher
    {
        public IStyleMatchContext? Context { get; set; }

        public ILookup<Type, IStyle> Styles { get; set; }

        readonly DotvvmConfiguration configuration;


        public StyleMatcher(IEnumerable<IStyle> styles, DotvvmConfiguration configuration)
        {
            Styles = styles.ToLookup(s => s.ControlType);
            this.configuration = configuration;
        }

        public void PushControl(ResolvedControl control)
        {
            Context = new StyleMatchContext<DotvvmBindableObject>(Context, control, configuration);
        }

        public void PopControl()
        {
            if (Context == null) throw new InvalidOperationException("Stack is already empty");
            Context = Context.Parent;
        }

        public IEnumerable<IStyleApplicator> GetMatchingStyles()
        {
            return GetStyleCandidatesForControl().Where(s => s.Matches(Context)).Select(s => s.Applicator);
        }

        protected IEnumerable<IStyle> GetStyleCandidatesForControl()
        {
            var type = Context!.Control.Metadata.Type;

            foreach (var s in GetImplicitStyles(type)) yield return s;
            foreach (var s in Styles[type]) yield return s;
            do
            {
                type = type.GetTypeInfo().BaseType;

                foreach (var s in Styles[type])
                    if (!s.ExactTypeMatch)
                        yield return s;

            } while (type != typeof(object));
        }

        private static ConcurrentDictionary<Type, ImmutableArray<IStyle>> implicitStyles = new ConcurrentDictionary<Type, ImmutableArray<IStyle>>();

        protected static IEnumerable<IStyle> GetImplicitStyles(Type controlType) =>
            implicitStyles.GetOrAdd(controlType, t => {
                if (t.IsAssignableFrom(typeof(DotvvmBindableObject))) return ImmutableArray<IStyle>.Empty;
                var configurationParameter = Expression.Parameter(typeof(DotvvmConfiguration));
                var controlParameter = Expression.Parameter(typeof(ResolvedControl));
                return GetImplicitStyles(t.GetTypeInfo().BaseType).Concat(
                    from m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    where m.IsDefined(typeof(ApplyControlStyleAttribute))
                    let parameters = m.GetParameters()
                    where parameters[0].ParameterType == typeof(ResolvedControl)
                    let invocationExpression = Expression.Call(m, new Expression[] { controlParameter }.Concat(
                        from p in parameters.Skip(1)
                        let services = Expression.Property(configurationParameter, nameof(DotvvmConfiguration.ServiceProvider))
                        select Expression.Call(
                            typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions),
                            nameof(IServiceProvider.GetService),
                            new[] { p.ParameterType },
                            services)))
                    let expression = Expression.Lambda<Action<ResolvedControl, DotvvmConfiguration>>(invocationExpression, controlParameter, configurationParameter)
                    select (IStyle)new GenericStyle(controlType, expression.Compile())
                    ).ToImmutableArray();
            });

        class GenericStyle : IStyle
        {
            public Type ControlType { get; }
            public Action<ResolvedControl, DotvvmConfiguration> Action { get; }

            public IStyleApplicator Applicator => new GenericApplicatpr(Action);

            public bool ExactTypeMatch => true;

            public GenericStyle(Type controlType, Action<ResolvedControl, DotvvmConfiguration> action)
            {
                this.ControlType = controlType;
                this.Action = action;
            }

            public bool Matches(IStyleMatchContext currentControl) => true;
        }

        class GenericApplicatpr : IStyleApplicator
        {
            private readonly Action<ResolvedControl, DotvvmConfiguration> action;

            public GenericApplicatpr(Action<ResolvedControl, DotvvmConfiguration> action)
            {
                this.action = action;
            }

            public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
            {
                try
                {
                    action(control, context.Configuration);
                }
                catch(Exception ex)
                {
                    control.DothtmlNode?.AddError($"Could not apply styles: {ex}");
                }
            }
        }
    }
}
