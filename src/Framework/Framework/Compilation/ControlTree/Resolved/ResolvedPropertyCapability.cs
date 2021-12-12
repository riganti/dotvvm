using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyCapability : ResolvedPropertySetter
    {
        public new DotvvmCapabilityProperty Property
        {
            get => (DotvvmCapabilityProperty)base.Property;
            set => base.Property = value;
        }

        public Type Type => Property.PropertyType;

        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Values { get; set; }
        public ImmutableArray<(PropertyInfo, DotvvmProperty)>? Mapping { get; set; }

        public ResolvedPropertyCapability(
            DotvvmCapabilityProperty property,
            Dictionary<DotvvmProperty, ResolvedPropertySetter> values,
            ImmutableArray<(PropertyInfo, DotvvmProperty)>? mapping = null) : base(property)
        {
            this.Values = values;
            Mapping = mapping;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            // not really needed, this does not occur in the tree so it does not make sense to add this to the visitor
            foreach (var v in Values.Values)
            {
                v.Accept(visitor);
            }
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor) { }

        public object? ToCapabilityObject(bool throwExceptions = false)
        {
            var capability = this.Property;

            if ((this.Mapping ?? capability.PropertyMapping) is not {} mapping)
            {
                if (throwExceptions)
                    throw new NotSupportedException($"Capability {capability} does not have property mapping.");
                else
                    return null;
            }

            var obj = Activator.CreateInstance(capability.PropertyType);
            object? convertValue(object? value, Type t)
            {
                t = t.UnwrapNullableType();
                if (t.IsInstanceOfType(value) || value is null)
                    return value;
                if (t.IsValueOrBinding(out var elementType))
                {
                    value = value as IBinding ?? convertValue(value, elementType);
                    if (value is IBinding)
                        return t.GetConstructor(new [] { typeof(IBinding) }).Invoke(new [] { value });
                    else
                        return t.GetConstructor(new [] { elementType }).Invoke(new [] { value });
                }
                // TODO: controls and templates
                if (throwExceptions)
                    throw new NotSupportedException($"Can not convert {value} to {t}");
                return null;
            }


            foreach (var (p, dotprop) in mapping)
            {
                if (this.Values.TryGetValue(dotprop, out var value))
                    p.SetValue(obj, convertValue(value.GetValue(), p.PropertyType));
            }

            if (capability.PropertyGroupMapping is not { Length: > 0 } groupMappingList)
                return obj;

            var propertyGroupLookup =
                this.Values.Keys
                    .OfType<GroupedDotvvmProperty>()
                    .ToLookup(gp => gp.PropertyGroup);

            foreach (var (prop, pgroup) in groupMappingList)
            {
                var properties = propertyGroupLookup[pgroup].ToArray();

                var propertyOriginalValue = prop.GetValue(obj);
                var dictionaryElementType = DotvvmCapabilityProperty.Helpers.GetDictionaryElement(prop.PropertyType);
                var dictionary = (System.Collections.IDictionary)(
                    propertyOriginalValue ??
                    Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(string), dictionaryElementType))
                );

                if (properties.Length > 0)
                {

                    foreach (var p in properties)
                        dictionary.Add(p.GroupMemberName, convertValue(this.Values[p].GetValue(), dictionaryElementType));
                }
                if (propertyOriginalValue is null)
                    prop.SetValue(obj, dictionary);
            }

            return obj;
        }

        private static string DebugFormatValue(object? v) =>
            v is null ? "null" :
            v is IEnumerable<object> vs ? $"[{string.Join(", ", vs.Select(DebugFormatValue))}]" :
            v.ToString();

        public override string ToString() =>
            $"{{{string.Join(", ", Values.Select(x => x.Key.Name + "." + DebugFormatValue(x.Value)))}}}";
    }
}
