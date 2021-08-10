﻿#nullable enable

using DotVVM.Framework.Binding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Collections;
using DotVVM.Framework.Utils;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class DotvvmPropertyGroup : IPropertyGroupDescriptor
    {
        public PropertyInfo? PropertyInfo { get; }

        public FieldInfo? DescriptorField { get; }

        public string[] Prefixes { get; }

        public string Name { get; }

        public MarkupOptionsAttribute? MarkupOptions { get; private set; }

        public DataContextChangeAttribute[] DataContextChangeAttributes { get; private set; }
            = new DataContextChangeAttribute[] {};

        public DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; private set; }

        public object DefaultValue { get; }

        public Type DeclaringType { get; }
        ITypeDescriptor IControlAttributeDescriptor.DeclaringType => ResolvedTypeDescriptor.Create(DeclaringType);

        public Type PropertyType { get; }
        ITypeDescriptor IControlAttributeDescriptor.PropertyType => ResolvedTypeDescriptor.Create(PropertyType);

        public Type? CollectionType { get; }
        ITypeDescriptor? IPropertyGroupDescriptor.CollectionType => ResolvedTypeDescriptor.Create(CollectionType);

        public PropertyGroupMode PropertyGroupMode { get; }

        public bool IsObsolete { get; private set; }

        public string? WorkaroundMessage { get; private set; }

        private ConcurrentDictionary<string, DotvvmProperty> generatedProperties = new ConcurrentDictionary<string, DotvvmProperty>();

        protected DotvvmPropertyGroup(PropertyInfo propertyInfo, PrefixArray prefixes, Type valueType, object defaultValue)
        {
            this.PropertyInfo = propertyInfo;
            this.DeclaringType = propertyInfo.DeclaringType;
            this.CollectionType = propertyInfo.PropertyType;
            this.Name = propertyInfo.Name;
            this.PropertyType = valueType;
            this.Prefixes = prefixes.Values;
            this.PropertyGroupMode = PropertyGroupMode.ValueCollection;
            this.DefaultValue = defaultValue;
            InitializePropertyGroup(propertyInfo);
        }

        protected DotvvmPropertyGroup(PrefixArray prefixes, Type valueType, FieldInfo descriptorField, string name, object defaultValue)
        {
            this.PropertyInfo = null;
            this.DescriptorField = descriptorField;
            this.DeclaringType = descriptorField.DeclaringType;
            this.CollectionType = null;
            this.Name = name;
            this.PropertyType = valueType;
            this.Prefixes = prefixes.Values;
            this.PropertyGroupMode = PropertyGroupMode.GeneratedDotvvmProperty;
            InitializePropertyGroup(descriptorField);
        }

        private void InitializePropertyGroup(ICustomAttributeProvider attributeProvider)
        {
            var markupOptions = MarkupOptions = attributeProvider.GetCustomAttribute<MarkupOptionsAttribute>(true)
                ?? new MarkupOptionsAttribute();
            var dataContextChange = attributeProvider.GetCustomAttributes<DataContextChangeAttribute>(true);
            var dataContextManipulation = attributeProvider
                .GetCustomAttribute<DataContextStackManipulationAttribute>(true);
            if (dataContextManipulation != null && dataContextChange.Any()) {
                throw new ArgumentException($"{nameof(DataContextChangeAttributes)} and "
                    + $"{nameof(DataContextManipulationAttribute)} can not be set both at property '{Name}'.");
            }

            DataContextChangeAttributes = dataContextChange.ToArray();
            DataContextManipulationAttribute = dataContextManipulation;

            var obsoleteAttribute = attributeProvider.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute != null)
            {
                IsObsolete = true;
                WorkaroundMessage = obsoleteAttribute.Message;
            }
        }

        IPropertyDescriptor IPropertyGroupDescriptor.GetDotvvmProperty(string name) => GetDotvvmProperty(name);
        public DotvvmProperty GetDotvvmProperty(string name)
        {
            return generatedProperties.GetOrAdd(name, n => GroupedDotvvmProperty.Create(this, name));
        }

        public static (Type valueType, MethodBase constructor) GetValueType(Type declaringType)
        {
            var collectionCtors = (from ctor in declaringType.GetConstructors()
                                   let parameters = ctor.GetParameters()
                                   where parameters.Length == 1 && typeof(IEnumerable).IsAssignableFrom(parameters[0].ParameterType)
                                   let elementType = ReflectionUtils.GetEnumerableType(parameters[0].ParameterType)
                                   where elementType.GetTypeInfo().IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                                   let genArguments = elementType.GetGenericArguments()
                                   where genArguments[0].IsAssignableFrom(typeof(string))
                                   let valueType = genArguments[1]
                                   select new { ctor, parameters, valueType }).ToArray();
            if (collectionCtors.Length >= 1)
            {
                return (collectionCtors[0].valueType, collectionCtors[0].ctor);
            }

            throw new NotSupportedException($"Could not initialize {declaringType.Name} as property group collection - no suitable constructor found");
        }

        private static ConcurrentDictionary<string, DotvvmPropertyGroup> descriptorDictionary = new ConcurrentDictionary<string, DotvvmPropertyGroup>();

        public static DotvvmPropertyGroup Create(PropertyInfo propertyInfo, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(propertyInfo.DeclaringType.Name + "." + propertyInfo.Name, fullName => {
                var attribute = propertyInfo.GetCustomAttribute<PropertyGroupAttribute>();
                var valueType = attribute.ValueType ?? GetValueType(propertyInfo.PropertyType).valueType;
                return new DotvvmPropertyGroup(propertyInfo, attribute.Prefixes, valueType, defaultValue);
            });
        }

        public static DotvvmPropertyGroup Register<TValue, TDeclaring>(PrefixArray prefixes, string name, TValue defaultValue = default(TValue)) =>
            Register(typeof(TDeclaring), prefixes, name, typeof(TValue), defaultValue);

        public static DotvvmPropertyGroup Register(Type declaringType, PrefixArray prefixes, string name, Type valueType, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(declaringType.Name + "." + name, fullName => {
                var descriptorField = FindDescriptorField(declaringType, name);
                return new DotvvmPropertyGroup(prefixes, valueType, descriptorField, name, defaultValue);
            });
        }

        internal static FieldInfo FindDescriptorField(Type declaringType, string name)
        {
            var fieldName = name + "GroupDescriptor";
            var field = declaringType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (field == null) throw new InvalidOperationException($"Could not declare property group '{declaringType.Name}.{name}' because backing field {fieldName} was not found.");
            return field;
        }

        internal static DotvvmPropertyGroup Register(DotvvmPropertyGroup group)
        {
            var key = @group.DeclaringType.Name + "." + @group.Name;
            if (descriptorDictionary.ContainsKey(key))
            {
                throw new InvalidOperationException($"The property group {key} is already registered!");
            }
            descriptorDictionary[key] = group;
            return group;
        }

        public static IEnumerable<DotvvmPropertyGroup> FindAttachedPropertyCandidates(string typeName)
        {
            return descriptorDictionary.Values
                .Where(pg => pg.PropertyGroupMode == PropertyGroupMode.GeneratedDotvvmProperty
                             && pg.DeclaringType.Name == typeName);
        }

        public static IPropertyDescriptor ResolvePropertyGroup(string name, bool caseSensitive)
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
            foreach (var property in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.IsDefined(typeof(PropertyGroupAttribute)))
                {
                    yield return Create(property, null);
                }
            }

            foreach (var pg in descriptorDictionary.Values)
            {
                if (pg.PropertyGroupMode == PropertyGroupMode.GeneratedDotvvmProperty && pg.DeclaringType.IsAssignableFrom(controlType))
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

    public enum PropertyGroupMode : byte
    {
        /// <summary> Property group that is set into a real property with a Dictionary or so. Something like a virtual dotvvm property. </summary>
        ValueCollection,
        /// <summary> Properties are backend in DotvvmControl.properties and accessed through VirtualPropertyGroupDictionary </summary>
        GeneratedDotvvmProperty
    }
}
