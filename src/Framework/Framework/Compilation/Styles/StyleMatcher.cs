﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using DotVVM.Framework.Configuration;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleMatcher
    {
        public IStyleMatchContext? Context { get; set; }

        public ILookup<Type, IStyle> Styles { get; set; }

        readonly DotvvmConfiguration configuration;
        int depth = 0;


        public StyleMatcher(IEnumerable<IStyle> styles, DotvvmConfiguration configuration)
        {
            Styles = styles.ToLookup(s => s.ControlType);
            this.configuration = configuration;
        }

        public void PushControl(ResolvedControl control)
        {
            depth++;
            Context = new StyleMatchContext<DotvvmBindableObject>(Context, control, configuration);
            if (depth > 100)
            {
                var controlsStack =
                    new [] { Context }.Concat(Context.GetAncestors())
                    .Select(c => c.Control.Metadata.Type.Name);
                throw new Exception($"Control hierarchy is unreasonably deep, there is probably an infinite cycle in server-side styles. This is the control hierarchy: {string.Join(", ", controlsStack)}");
            }
        }

        public void PopControl()
        {
            if (Context == null) throw new InvalidOperationException("Stack is already empty");
            depth--;
            Context = Context.Parent;
        }

        public IEnumerable<IStyleApplicator> GetMatchingStyles()
        {
            return GetStyleCandidatesForControl().Where(s => s.Matches(Context.NotNull("not initialized"))).Select(s => s.Applicator);
        }

        protected IEnumerable<IStyle> GetStyleCandidatesForControl()
        {
            var type = Context!.Control.Metadata.Type;

            foreach (var s in GetImplicitStyles(type)) yield return s;
            foreach (var s in Styles[type]) yield return s;
            do
            {
                type = type.BaseType!;

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
                return GetImplicitStyles(t.BaseType!).Concat(
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
                    select (IStyle)new GenericStyle(controlType, expression.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression))
                    ).ToImmutableArray();
            });

        /// <summary> Clear cache when hot reload happens </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                implicitStyles.TryRemove(t, out _);
        }


        class GenericStyle : IStyle
        {
            public Type ControlType { get; }
            public Action<ResolvedControl, DotvvmConfiguration> Action { get; }

            public IStyleApplicator Applicator => new GenericApplicator(Action);

            public bool ExactTypeMatch => true;

            public GenericStyle(Type controlType, Action<ResolvedControl, DotvvmConfiguration> action)
            {
                this.ControlType = controlType;
                this.Action = action;
            }

            public bool Matches(IStyleMatchContext currentControl) => true;
        }

        class GenericApplicator : IStyleApplicator
        {
            private readonly Action<ResolvedControl, DotvvmConfiguration> action;

            public GenericApplicator(Action<ResolvedControl, DotvvmConfiguration> action)
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
