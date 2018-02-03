using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using System.Reflection;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using DotVVM.Framework.Binding.Expressions;
using System.Diagnostics;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    public abstract class EpicCoolControl : DotvvmControl
    {
        public EpicCoolControl()
        {
        }

        private class ControlInfo
        {
            public MethodInfo RenderMethod;
            public ImmutableArray<Func<IDotvvmRequestContext, EpicCoolControl, object>> Properties;
        }
        private static ConcurrentDictionary<Type, ControlInfo> controlInfoCache = new ConcurrentDictionary<Type, ControlInfo>();

        private static object registrationLock = new object();
        internal static void RegisterProperties(Type controlType)
        {
            Func<IDotvvmRequestContext, EpicCoolControl, object> initializeArgument(ParameterInfo parameter)
            {
                if (parameter.ParameterType == typeof(IDotvvmRequestContext))
                    return (context, _) => context;

                if (parameter.GetCustomAttribute<PropertyGroupAttribute>() is PropertyGroupAttribute groupAttribute)
                {
                    // get value type from dictionary
                    var elementType =
                        parameter.ParameterType.GetGenericTypeDefinition() == typeof(VirtualPropertyGroupDictionary<>) ?
                            parameter.ParameterType.GetGenericArguments()[0] :
                        parameter.ParameterType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ||parameter.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ?
                            parameter.ParameterType.GetGenericArguments()
                            .Assert(p => p[0] == typeof(string))
                            [0] :
                        throw new NotSupportedException($"{parameter.ParameterType.FullName} is not supported property group type");


                    var propertyGroup = DotvvmPropertyGroup.Register(
                        controlType,
                        groupAttribute.Prefixes,
                        parameter.Name,
                        elementType,
                        parameter as ICustomAttributeProvider,
                        null
                    );

                    return (_, control) =>
                        typeof(VirtualPropertyGroupDictionary<>)
                        .MakeGenericType(new [] { elementType })
                        .GetConstructor(new [] { typeof(DotvvmBindableObject), typeof(DotvvmPropertyGroup) })
                        .Invoke(new object[] { control, propertyGroup });
                }
                else
                {
                    var type =
                        typeof(ValueOrBinding).IsAssignableFrom(parameter.ParameterType) ?
                        (
                            parameter.ParameterType.IsGenericType ?
                            parameter.ParameterType.GenericTypeArguments.Single() :
                            typeof(object)
                        ) :
                        parameter.ParameterType;

                    var dotvvmProperty = new DotvvmProperty();
                    DotvvmProperty.Register(parameter.Name, type, controlType, parameter.DefaultValue, false, dotvvmProperty, parameter);

                    if (!parameter.HasDefaultValue)
                        dotvvmProperty.MarkupOptions.Required = true;

                    if (typeof(IBinding).IsAssignableFrom(parameter.ParameterType))
                        dotvvmProperty.MarkupOptions.AllowHardCodedValue = false;
                    else if (!typeof(ValueOrBinding).IsAssignableFrom(parameter.ParameterType))
                        dotvvmProperty.MarkupOptions.AllowBinding = false;

                    if (typeof(DotvvmBindableObject).IsAssignableFrom(type))
                        dotvvmProperty.MarkupOptions.MappingMode = MappingMode.Both;

                    if (typeof(IBinding).IsAssignableFrom(parameter.ParameterType))
                        return (_, control) => control.GetBinding(dotvvmProperty);
                    else if (typeof(ValueOrBinding).IsAssignableFrom(parameter.ParameterType))
                        return (_, control) => typeof(DotvvmBindableObject).GetMethod("GetValueOrBinding").MakeGenericMethod(new [] { type }).Invoke(control, new object[]{ dotvvmProperty, true });
                    else return (_, control) => control.GetValue(dotvvmProperty);
                }
            }

            lock(registrationLock)
            {
                if (controlInfoCache.ContainsKey(controlType)) return;

                var method = controlType.GetMethod("GetContents", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                if (method == null)
                    throw new Exception($"Could not initialize control {controlType.FullName}, could not find (single) GetContents method");
                if (!(typeof(DotvvmControl).IsAssignableFrom(method.ReturnType) || typeof(IEnumerable<DotvvmControl>).IsAssignableFrom(method.ReturnType)))
                    throw new Exception($"Could not initialize control {controlType.FullName}, GetContents method does not return DotvvmControl nor IEnumerable<DotvvmControl>");

                var arguments = method.GetParameters().Select(initializeArgument);

                if (!controlInfoCache.TryAdd(controlType, new ControlInfo { RenderMethod = method, Properties = arguments.ToImmutableArray() }))
                    throw new Exception("no");
            }
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            var info = controlInfoCache[this.GetType()];

            // TODO: generate Linq.Expression instead of this reflection invocation
            var args = info.Properties.Select(p => p(context, this)).ToArray();
            var content = info.RenderMethod.Invoke(this, args);

            if (content is IEnumerable<DotvvmControl> enumerable)
                foreach (var c in enumerable) this.Children.Add(c);
            else if (content != null)
                this.Children.Add((DotvvmControl)content);

            base.OnLoad(context);
        }
    }
}