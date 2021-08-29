using DotVVM.Framework.Binding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.ControlTree
{
public class DotvvmPropertyGroup : IPropertyGroupDescriptor
{
    public FieldInfo DescriptorField { get; }

    public ICustomAttributeProvider AttributeProvider { get; }

    public string[] Prefixes { get; }

    public string Name { get; }

    public MarkupOptionsAttribute MarkupOptions { get; }

    public DataContextChangeAttribute[] DataContextChangeAttributes { get; }

    public DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; }

    public object? DefaultValue { get; }

    public Type DeclaringType { get; }
    ITypeDescriptor IControlAttributeDescriptor.DeclaringType => new ResolvedTypeDescriptor(DeclaringType);

    public Type PropertyType { get; }
    ITypeDescriptor IControlAttributeDescriptor.PropertyType => new ResolvedTypeDescriptor(PropertyType);
    public IAttributeValueMerger? ValueMerger { get; }

        private ConcurrentDictionary<string, GroupedDotvvmProperty> generatedProperties = new();

        /// <summary> The capability which declared this property. When the property is declared by an capability, it can only be used by this capability. </summary>
        public DotvvmCapabilityProperty? OwningCapability { get; internal set; }
        /// <summary> The capabilities which use this property. </summary>
        public ImmutableArray<DotvvmCapabilityProperty> UsedInCapabilities { get; internal set; } = ImmutableArray<DotvvmCapabilityProperty>.Empty;


        internal DotvvmPropertyGroup(PrefixArray prefixes, Type valueType, Type declaringType, FieldInfo descriptorField, ICustomAttributeProvider attributeProvider, string name, object? defaultValue)
        {
            this.DescriptorField = descriptorField;
            this.DeclaringType = declaringType;
            this.AttributeProvider = attributeProvider;
            this.Name = name;
            this.PropertyType = valueType;
            this.Prefixes = prefixes.Values;
            (this.MarkupOptions, this.DataContextChangeAttributes, this.DataContextManipulationAttribute) = InitFromAttributes(attributeProvider, name);
            if (MarkupOptions.AllowValueMerging)
            {
                ValueMerger = (IAttributeValueMerger)Activator.CreateInstance(MarkupOptions.AttributeValueMerger);
            }
        }

        private static (MarkupOptionsAttribute, DataContextChangeAttribute[], DataContextStackManipulationAttribute?) InitFromAttributes(ICustomAttributeProvider attributeProvider, string name)
        {
            var markupOptions = attributeProvider.GetCustomAttribute<MarkupOptionsAttribute>(true) ?? new MarkupOptionsAttribute();
            var dataContextChange = attributeProvider.GetCustomAttributes<DataContextChangeAttribute>(true);
            var dataContextManipulation = attributeProvider.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
            if (dataContextManipulation != null && dataContextChange.Any()) throw new ArgumentException(
                $"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property group '{name}'.");
            return (markupOptions, dataContextChange.ToArray(), dataContextManipulation);
        }

        IPropertyDescriptor IPropertyGroupDescriptor.GetDotvvmProperty(string name) => GetDotvvmProperty(name);
        public GroupedDotvvmProperty GetDotvvmProperty(string name)
        {
            return generatedProperties.GetOrAdd(name, n => GroupedDotvvmProperty.Create(this, name));
        }

        private static ConcurrentDictionary<(Type, string), DotvvmPropertyGroup> descriptorDictionary = new();

        public static DotvvmPropertyGroup Register<TValue, TDeclaring>(PrefixArray prefixes, string name, TValue? defaultValue = default(TValue)) =>
            Register(typeof(TDeclaring), prefixes, name, typeof(TValue), defaultValue);

        public static DotvvmPropertyGroup  Register(Type declaringType, PrefixArray prefixes, string name, Type valueType, object? defaultValue)
        {
            return descriptorDictionary.GetOrAdd((declaringType, name), fullName => {
                var field = declaringType.GetField(name + "GroupDescriptor", BindingFlags.Public | BindingFlags.Static);
                if (field == null) throw new InvalidOperationException($"Could not declare property group '{fullName}' because backing field was not found.");
                return new DotvvmPropertyGroup(prefixes, valueType, declaringType, field, field as ICustomAttributeProvider, name, defaultValue);
            });
        }
        internal static DotvvmPropertyGroup Register(DotvvmPropertyGroup group)
        {
            var key = (@group.DeclaringType, @group.Name);
            if (descriptorDictionary.ContainsKey(key))
            {
                throw new InvalidOperationException($"The property group {key} is already registered!");
            }
            descriptorDictionary[key] = group;
            return group;
        }
        internal static FieldInfo FindDescriptorField(Type declaringType, string name)
        {
            var fieldName = name + "GroupDescriptor";
            var field = declaringType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (field == null) throw new InvalidOperationException($"Could not declare property group '{declaringType.Name}.{name}' because backing field {fieldName} was not found.");
            return field;
        }

        public static DotvvmPropertyGroup Register(Type declaringType, PrefixArray prefixes, string name, Type valueType, ICustomAttributeProvider attributeProvider, object? defaultValue)
        {
            return descriptorDictionary.GetOrAdd((declaringType, name), fullName => {
                // the field is optional, here
                var field = declaringType.GetField(name + "GroupDescriptor", BindingFlags.Public | BindingFlags.Static);
                return new DotvvmPropertyGroup(prefixes, valueType, declaringType, field, attributeProvider, name, defaultValue);
            });
        }

        public static IEnumerable<DotvvmPropertyGroup> FindAttachedPropertyCandidates(string typeName)
        {
            return descriptorDictionary.Values
                .Where(pg => pg.DeclaringType.Name == typeName);
        }

        public static IPropertyDescriptor? ResolvePropertyGroup(string name, bool caseSensitive)
        {
            var nameParts = name.Split('.');
            var groups = FindAttachedPropertyCandidates(nameParts[0])
                .SelectMany(g => g.Prefixes.Select(p => new { Group = g, Prefix = p }));

            var group = groups
                .FirstOrDefault(g => nameParts[1].StartsWith(g.Prefix, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
            if (group != null)
            {
                var concreteName = nameParts[1].Substring(group.Prefix.Length);
                return group.Group.GetDotvvmProperty(concreteName);
            }

            return null;
        }

        static void RunClassConstructor(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            if (type.BaseType != typeof(object))
                RunClassConstructor(type.BaseType);
        }

        public static IEnumerable<DotvvmPropertyGroup> GetPropertyGroups(Type controlType)
        {
            RunClassConstructor(controlType);
            foreach (var pg in descriptorDictionary.Values)
            {
                if (pg.DeclaringType.IsAssignableFrom(controlType))
                {
                    yield return pg;
                }
            }
        }

        public struct PrefixArray
        {
            public readonly string[] Values;

            public PrefixArray(string[] values)
            {
                this.Values = values;
            }

            public PrefixArray(string value)
            {
                this.Values = new[] { value };
            }

            public static implicit operator PrefixArray(string val) => new PrefixArray(val);
            public static implicit operator PrefixArray(string[] val) => new PrefixArray(val);
        }
    }
}
