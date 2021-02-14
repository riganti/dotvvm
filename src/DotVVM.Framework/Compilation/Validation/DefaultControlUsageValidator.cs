using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Validation
{
    public class DefaultControlUsageValidator : IControlUsageValidator
    {
        public DefaultControlUsageValidator(DotvvmConfiguration config)
        {
            Configuration = config;
        }

        public static ConcurrentDictionary<Type, MethodInfo[]> cache = new ConcurrentDictionary<Type, MethodInfo[]>();

        protected static DotvvmConfiguration Configuration { get; private set; }

        public IEnumerable<ControlUsageError> Validate(IAbstractControl control)
        {
            var type = GetControlType(control.Metadata);
            if (type == null) return null;

            var result = new List<ControlUsageError>();
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
            var ancestorMethods = FindMethods(type.GetTypeInfo().BaseType);
            return ancestorMethods.Concat(methods).ToArray();
        }

        protected virtual Type GetControlType(IControlResolverMetadata metadata)
        {
            var type = metadata.Type as ResolvedTypeDescriptor;
            return type?.Type;
        }
    }
}
