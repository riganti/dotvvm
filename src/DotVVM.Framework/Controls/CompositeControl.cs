using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using System.Reflection;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using DotVVM.Framework.Binding.Expressions;
using System.Diagnostics;
using System.Linq.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for controls implemented using other components returned from the `GetContents` method
    /// </summary>
    public abstract class CompositeControl : DotvvmControl
    {
        public CompositeControl()
        {
        }

        private class ControlInfo
        {
            public MethodInfo GetContentsMethod;
            public ImmutableArray<Func<IDotvvmRequestContext, CompositeControl, object>> Properties;
        }
        private static ConcurrentDictionary<Type, ControlInfo> controlInfoCache = new ConcurrentDictionary<Type, ControlInfo>();

        private static object registrationLock = new object();
        internal static void RegisterProperties(Type controlType)
        {
            Func<IDotvvmRequestContext, CompositeControl, object> initializeArgument(ParameterInfo parameter)
            {
                if (parameter.ParameterType == typeof(IDotvvmRequestContext))
                    return (context, _) => context;
                var (getter, setter) = DotvvmCapabilityProperty.InitializeArgument(parameter, parameter.Name, parameter.ParameterType, controlType, null);

                var wrappedExpression = Expression.Lambda<Func<IDotvvmRequestContext, CompositeControl, object>>(
                    Expression.Convert(getter.Body, typeof(object)),
                    Expression.Parameter(typeof(IDotvvmRequestContext)), getter.Parameters[0]);
                return wrappedExpression.Compile();
            }

            lock (registrationLock)
            {
                if (controlInfoCache.ContainsKey(controlType)) return;

                var method = controlType.GetMethod("GetContents", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                if (method == null)
                    throw new Exception($"Could not initialize control {controlType.FullName}, could not find (single) GetContents method");
                if (!(typeof(DotvvmControl).IsAssignableFrom(method.ReturnType) || typeof(IEnumerable<DotvvmControl>).IsAssignableFrom(method.ReturnType)))
                    throw new Exception($"Could not initialize control {controlType.FullName}, GetContents method does not return DotvvmControl nor IEnumerable<DotvvmControl>");

                var arguments = method.GetParameters().Select(initializeArgument);

                if (!controlInfoCache.TryAdd(controlType, new ControlInfo { GetContentsMethod = method, Properties = arguments.ToImmutableArray() }))
                    throw new Exception("no");
            }
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            var info = controlInfoCache[this.GetType()];

            // TODO: generate Linq.Expression instead of this reflection invocation
            var args = info.Properties.Select(p => p(context, this)).ToArray();
            var content = info.GetContentsMethod.Invoke(this, args);

            if (content is IEnumerable<DotvvmControl> enumerable)
                foreach (var c in enumerable) this.Children.Add(c);
            else if (content != null)
                this.Children.Add((DotvvmControl)content);

            base.OnLoad(context);
        }
    }
}
