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
using DotVVM.Framework.Compilation;
using FastExpressionCompiler;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for controls implemented using other components returned from the `GetContents` method
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public abstract class CompositeControl : DotvvmControl
    {
        public CompositeControl()
        {
        }

        internal class ControlInfo
        {
            public MethodInfo GetContentsMethod;
            public ImmutableArray<Func<IDotvvmRequestContext, CompositeControl, object>> Getters;
            public ImmutableArray<IControlAttributeDescriptor> Properties { get; }

            public ControlInfo(MethodInfo getContentsMethod, ImmutableArray<Func<IDotvvmRequestContext, CompositeControl, object>> getters, ImmutableArray<IControlAttributeDescriptor> properties)
            {
                GetContentsMethod = getContentsMethod;
                Getters = getters;
                Properties = properties;
            }
        }

        // TODO: clear on hot reload
        private static ConcurrentDictionary<Type, ControlInfo> controlInfoCache = new ConcurrentDictionary<Type, ControlInfo>();

        private static object registrationLock = new object();
        internal static void RegisterProperties(Type controlType)
        {
            IControlAttributeDescriptor initializeArgument(Type controlType, ParameterInfo parameter)
            {
                if (parameter.ParameterType == typeof(IDotvvmRequestContext))
                    return Internal.RequestContextProperty;
                var defaultValue =
                    parameter.HasDefaultValue ?
                        (ValueOrBinding<object>?)ValueOrBinding<object>.FromBoxedValue(parameter.DefaultValue) :
                        (ValueOrBinding<object>?)null;
                var newProperty = DotvvmCapabilityProperty.InitializeArgument(parameter, parameter.Name!, parameter.ParameterType, controlType, null, defaultValue);
                var newProperty = DotvvmCapabilityProperty.InitializeArgument(parameter, parameter.Name, parameter.ParameterType, controlType, null, defaultValue);
                return newProperty;
            }
            Func<IDotvvmRequestContext, CompositeControl, object> compileGetter(IControlAttributeDescriptor property, Type parameterType)
            {
                if (property == Internal.RequestContextProperty)
                    return (context, _) => context;

                var (getter, setter) =
                    property is DotvvmProperty p ? DotvvmCapabilityProperty.CodeGeneration.CreatePropertyAccessors(parameterType, p) :
                    property is DotvvmPropertyGroup g ? DotvvmCapabilityProperty.CodeGeneration.CreatePropertyGroupAccessors(parameterType, g) :
                    throw new NotSupportedException();

                var wrappedExpression = Expression.Lambda<Func<IDotvvmRequestContext, CompositeControl, object>>(
                    Expression.Convert(getter.Body, typeof(object)),
                    Expression.Parameter(typeof(IDotvvmRequestContext)), getter.Parameters[0]);
                return wrappedExpression.CompileFast();

            }

            lock (registrationLock)
            {
                if (controlInfoCache.ContainsKey(controlType)) return;

                DefaultControlResolver.InitType(controlType.BaseType.NotNull());

                var method = controlType.GetMethod("GetContents", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    throw new Exception($"Could not initialize control {controlType.FullName}, could not find (single) GetContents method");
                if (!(typeof(DotvvmControl).IsAssignableFrom(method.ReturnType) || typeof(IEnumerable<DotvvmControl>).IsAssignableFrom(method.ReturnType)))
                    throw new Exception($"Could not initialize control {controlType.FullName}, GetContents method does not return DotvvmControl nor IEnumerable<DotvvmControl>");

                var argumentProperties = method.GetParameters().Select(p => initializeArgument(controlType, p)).ToImmutableArray();
                var argumentGetters = argumentProperties.Zip(method.GetParameters(), (prop, arg) => compileGetter(prop, arg.ParameterType)).ToImmutableArray();

                if (!controlInfoCache.TryAdd(controlType, new ControlInfo(method, argumentGetters, argumentProperties)))
                    throw new Exception("no");
            }
        }

        static internal ControlInfo GetControlInfo(Type controlType) =>
            controlInfoCache[controlType];

        internal IEnumerable<DotvvmControl> ExecuteGetContents(IDotvvmRequestContext context)
        {
            var info = GetControlInfo(this.GetType());
            // TODO: generate Linq.Expression instead of this reflection invocation
            var args = info.Getters.Select(p => p(context, this)).ToArray();
            var result = info.GetContentsMethod.Invoke(this, args);

            if (result is IEnumerable<DotvvmControl> enumerable)
                return enumerable;
            else if (result != null)
                return new DotvvmControl[] { (DotvvmControl)result };
            else
                return Array.Empty<DotvvmControl>();
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            if (!this.HasOnlyWhiteSpaceContent())
                throw new DotvvmControlException(this, $"Cannot set children into {GetType().Name} which derives from CompositeControl. To set content of composite control, use a property of type DotvvmControl or ITemplate.");

            this.Children.Clear();

            var content = ExecuteGetContents(context);

            if (this.Children.Count > 0)
                throw new DotvvmControlException(this, $"{GetType().Name}.GetContents may not modify the Children collection, it should return the new children and it will be handled automatically.");

            foreach (var child in content)
                this.Children.Add(child);

            base.OnLoad(context);
        }
    }
}
