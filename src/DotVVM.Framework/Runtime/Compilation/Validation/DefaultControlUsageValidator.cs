using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.ControlTree;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using DotVVM.Framework.Runtime.ControlTree.Resolved;
using System.Reflection;

namespace DotVVM.Framework.Runtime.Compilation.Validation
{
    public class DefaultControlUsageValidator : IControlUsageValidator
    {
        public static ConcurrentDictionary<Type, MethodInfo[]> cache = new ConcurrentDictionary<Type, MethodInfo[]>();
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
                    else
                    {
                        goto Error; // I think it is better that throw exceptions and catch them
                    }
                }
                var r = method.Invoke(null, args);
                if(r is IEnumerable<ControlUsageError>)
                {
                    result.AddRange((IEnumerable<ControlUsageError>)r);
                }
                else if(r is IEnumerable<string>)
                {
                    result.AddRange((r as IEnumerable<string>).Select(e => new ControlUsageError(e, new[] { control.DothtmlNode })));
                }
                continue;
                Error:;
            }
            return result;
        }

        protected virtual MethodInfo[] FindMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(m => Attribute.IsDefined(m, typeof(ControlUsageValidatorAttribute)))
                .ToArray();
        }

        protected virtual Type GetControlType(IControlResolverMetadata metadata)
        {
            var type = metadata.Type as ResolvedTypeDescriptor;
            return type?.Type;
        }
    }
}
