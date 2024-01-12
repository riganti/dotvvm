using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Validation
{
    public class DefaultControlUsageValidator : IControlUsageValidator
    {
        public DefaultControlUsageValidator(DotvvmConfiguration config)
        {
            Configuration = config;
        }

        public static ConcurrentDictionary<Type, (MethodInfo[] controlValidators, MethodInfo[] attachedPropertyValidators)> cache = new();

        protected DotvvmConfiguration Configuration { get; }

        public static IEnumerable<ControlUsageError> ValidateDefaultRules(IAbstractControl control)
        {
            // check required properties
            var missingProperties = control.Metadata.AllProperties.Where(p => p.MarkupOptions.Required && !control.TryGetProperty(p, out _)).ToList();
            if (missingProperties.Any())
            {
                yield return new ControlUsageError(
                    $"The control '{ control.Metadata.Type.CSharpName }' is missing required properties: { string.Join(", ", missingProperties.Select(p => "'" + p.Name + "'")) }.",
                    control.DothtmlNode
                );
            }

            var unknownContent = control.Content.Where(c => !c.Metadata.Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmControl))));
            foreach (var unknownControl in unknownContent)
            {
                yield return new ControlUsageError(
                    $"The control '{ unknownControl.Metadata.Type.CSharpName }' does not inherit from DotvvmControl and thus cannot be used in content.",
                    control.DothtmlNode
                );
            }
        }

        private IEnumerable<ControlUsageError> CallMethod(MethodInfo method, IAbstractControl control)
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
                    return Enumerable.Empty<ControlUsageError>();
                }
            }
            var r = method.Invoke(null, args);
            if (r is null)
            {
                return Enumerable.Empty<ControlUsageError>();
            }
            else if (r is IEnumerable<ControlUsageError> errors)
            {
                return errors;
            }
            else if (r is IEnumerable<string> stringErrors)
            {
                return stringErrors.Select(e => new ControlUsageError(e));
            }
            else
            {
                throw new Exception($"ControlUsageValidator method '{ReflectionUtils.FormatMethodInfo(method)}' returned an invalid type. Expected IEnumerable<ControlUsageError> or IEnumerable<string>.");
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
            foreach (var method in methods.controlValidators)
            {
                result.AddRange(CallMethod(method, control));
            }

            var attachedPropertiesTypes = new HashSet<Type>();
            foreach (var attachedProperty in control.PropertyNames)
            {
                if (attachedProperty.DeclaringType.IsAssignableFrom(control.Metadata.Type))
                    continue; // not an attached property
                if (GetPropertyDeclaringType(attachedProperty) is {} declaringType)
                    attachedPropertiesTypes.Add(declaringType);
            }

            foreach (var attachedPropertyType in attachedPropertiesTypes)
            {
                var (_, attachedValidators) = cache.GetOrAdd(attachedPropertyType, FindMethods);
                foreach (var method in attachedValidators)
                {
                    result.AddRange(CallMethod(method, control));
                }
            }

            return result
                // add current node to the error, if no control is specified
                .Select(e => e.Nodes.Length == 0 ?
                    new ControlUsageError(e.ErrorMessage, e.Severity, control.DothtmlNode) :
                    e);
        }

        protected virtual (MethodInfo[] controlValidators, MethodInfo[] attachedPropertyValidators) FindMethods(Type type)
        {
            if (type == typeof(object))
                return (Array.Empty<MethodInfo>(), Array.Empty<MethodInfo>());

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.IsDefined(typeof(ControlUsageValidatorAttribute)))
                .ToArray();

            var attributes = methods.Select(method => (method, attr: method.GetCustomAttribute<ControlUsageValidatorAttribute>().NotNull())).ToList();
            var overrideValidation = attributes.Select(s => s.attr.Override).Distinct().ToList();

            if (overrideValidation.Count > 1)
                throw new Exception($"ControlUsageValidator attributes on '{type.FullName}' are in an inconsistent state. Make sure all attributes have an Override property set to the same value.");

            var attachedValidators = attributes.Where(s => s.attr.IncludeAttachedProperties).Select(m => m.method).ToArray();
                
            if (overrideValidation.Any() && overrideValidation[0])
                return (methods, attachedValidators);

            var ancestorMethods = FindMethods(type.BaseType!);
            // attached validators are not inherited
            return (ancestorMethods.controlValidators.Concat(methods).ToArray(), attachedValidators);
        }

        protected virtual Type? GetControlType(IControlResolverMetadata metadata)
        {
            var type = metadata.Type as ResolvedTypeDescriptor;
            return type?.Type;
        }

        protected virtual Type? GetPropertyDeclaringType(IPropertyDescriptor property)
        {
            if (property is DotvvmProperty p)
                return p.DeclaringType;
            return (property.DeclaringType as ResolvedTypeDescriptor)?.Type;
        }

        /// <summary> Clear cache when hot reload happens </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                cache.TryRemove(t, out _);
        }
    }
}
