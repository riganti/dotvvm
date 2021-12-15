using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.Validation
{
    public class DefaultControlUsageValidator : IControlUsageValidator
    {
        public DefaultControlUsageValidator(DotvvmConfiguration config)
        {
            Configuration = config;
        }

        public static ConcurrentDictionary<Type, MethodInfo[]> cache = new ConcurrentDictionary<Type, MethodInfo[]>();

        protected DotvvmConfiguration Configuration { get; }

        public static IEnumerable<ControlUsageError> ValidateDefaultRules(IAbstractControl control)
        {
            // check required properties
            var missingProperties = control.Metadata.AllProperties.Where(p => p.MarkupOptions.Required && !control.TryGetProperty(p, out _)).ToList();
            if (missingProperties.Any())
            {
                yield return new ControlUsageError(
                    $"The control '{ control.Metadata.Type.FullName }' is missing required properties: { string.Join(", ", missingProperties.Select(p => "'" + p.Name + "'")) }.",
                    control.DothtmlNode
                );
            }

            var unknownContent = control.Content.Where(c => !c.Metadata.Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmControl))));
            foreach (var unknownControl in unknownContent)
            {
                yield return new ControlUsageError(
                    $"The control '{ unknownControl.Metadata.Type.FullName }' does not inherit from DotvvmControl and thus cannot be used in content.",
                    control.DothtmlNode
                );
            }
        }

        public IEnumerable<ControlUsageError> Validate(IAbstractControl control)
        {
            var type = GetControlType(control.Metadata);
            if (type == null) return Enumerable.Empty<ControlUsageError>();

            var result = new List<ControlUsageError>();
            result.AddRange(ValidateDefaultRules(control));
            if (result.Any())
                return result;

            var methods = cache.GetOrAdd(type, FindMethods);
            foreach (var method in methods)
            {
                var par = method.GetParameters();
                var args = new object[par.Length];
                for (int i = 0; i < par.Length; i++)
                {
                    if (par[i].ParameterType.IsAssignableFrom(control.GetType()))
                    {
                        args[i] = control;
                    }
                    else if (control.DothtmlNode != null && par[i].ParameterType.IsAssignableFrom(control.DothtmlNode.GetType()))
                    {
                        args[i] = control.DothtmlNode;
                    }
                    else if (par[i].ParameterType == typeof(DotvvmConfiguration))
                    {
                        args[i] = Configuration;
                    }
                    else
                    {
                        goto Error; // I think it is better that throw exceptions and catch them
                    }
                }
                var r = method.Invoke(null, args);
                if (r is IEnumerable<ControlUsageError>)
                {
                    result.AddRange((IEnumerable<ControlUsageError>)r);
                }
                else if (r is IEnumerable<string>)
                {
                    result.AddRange((r as IEnumerable<string>).Select(e => new ControlUsageError(e)));
                }
                continue;
                Error:;
            }

            return result
                // add current node to the error, if no control is specified
                .Select(e => e.Nodes.Length == 0 ?
                    new ControlUsageError(e.ErrorMessage, control.DothtmlNode) :
                    e);
        }

        protected virtual MethodInfo[] FindMethods(Type type)
        {
            if (type == typeof(object)) return new MethodInfo[0];
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.IsDefined(typeof(ControlUsageValidatorAttribute)))
                .ToArray();

            var attributes = methods.Select(s => s.GetCustomAttribute(typeof(ControlUsageValidatorAttribute))).ToList();
            var overrideValidation = attributes.OfType<ControlUsageValidatorAttribute>().Select(s => s.Override).Distinct().ToList();

            if (overrideValidation.Count > 1)
                throw new Exception($"ControlUsageValidator attributes on '{type.FullName}' are in an inconsistent state. Make sure all attributes have an Override property set to the same value.");

            if (overrideValidation.Any() && overrideValidation[0]) return methods;
            var ancestorMethods = FindMethods(type.BaseType);
            return ancestorMethods.Concat(methods).ToArray();
        }

        protected virtual Type? GetControlType(IControlResolverMetadata metadata)
        {
            var type = metadata.Type as ResolvedTypeDescriptor;
            return type?.Type;
        }

        /// <summary> Clear cache when hot reload happens </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                cache.TryRemove(t, out _);
        }
    }
}
