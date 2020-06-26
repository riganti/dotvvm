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
        public PropertyInfo PropertyInfo { get; }

        public FieldInfo DescriptorField { get; }

        public string[] Prefixes { get; }

        public string Name { get; }

        public MarkupOptionsAttribute MarkupOptions { get; }

        public DataContextChangeAttribute[] DataContextChangeAttributes { get; }

        public DataContextStackManipulationAttribute DataContextManipulationAttribute { get; }

        public object DefaultValue { get; }

        public Type DeclaringType { get; }
        ITypeDescriptor IControlAttributeDescriptor.DeclaringType => new ResolvedTypeDescriptor(DeclaringType);

        public Type PropertyType { get; }
        ITypeDescriptor IControlAttributeDescriptor.PropertyType => new ResolvedTypeDescriptor(PropertyType);

        public Type CollectionType { get; }
        ITypeDescriptor IPropertyGroupDescriptor.CollectionType => new ResolvedTypeDescriptor(CollectionType);

        public PropertyGroupMode PropertyGroupMode { get; }

        private ConcurrentDictionary<string, DotvvmProperty> generatedProperties = new ConcurrentDictionary<string, DotvvmProperty>();

        private DotvvmPropertyGroup (PropertyInfo propertyInfo, PrefixArray prefixes, Type valueType, object defaultValue)
        {
            this.PropertyInfo = propertyInfo;
            this.DeclaringType = propertyInfo.DeclaringType;
            this.CollectionType = propertyInfo.PropertyType;
            this.Name = propertyInfo.Name;
            this.PropertyType = valueType;
            this.Prefixes = prefixes.Values;
            this.PropertyGroupMode = PropertyGroupMode.ValueCollection;
            this.DefaultValue = defaultValue;

            var markupOptions = this.MarkupOptions = propertyInfo.GetCustomAttribute<MarkupOptionsAttribute>(true) ?? new MarkupOptionsAttribute();
            var dataContextChange = propertyInfo.GetCustomAttributes<DataContextChangeAttribute>(true);
            var dataContextManipulation = propertyInfo.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
            if (dataContextManipulation != null && dataContextChange.Any()) throw new ArgumentException(
                $"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property '{propertyInfo.Name}'.");
            DataContextChangeAttributes = dataContextChange.ToArray();
            DataContextManipulationAttribute = dataContextManipulation;
        }

        private DotvvmPropertyGroup (PrefixArray prefixes, Type valueType, FieldInfo descriptorField, string name, object defaultValue)
        {
            this.PropertyInfo = null;
            this.DescriptorField = descriptorField;
            this.DeclaringType = descriptorField.DeclaringType;
            this.CollectionType = null;
            this.Name = name;
            this.PropertyType = valueType;
            this.Prefixes = prefixes.Values;
            this.PropertyGroupMode = PropertyGroupMode.GeneratedDotvvmProperty;

            var markupOptions = this.MarkupOptions = descriptorField.GetCustomAttribute<MarkupOptionsAttribute>(true) ?? new MarkupOptionsAttribute();
            var dataContextChange = descriptorField.GetCustomAttributes<DataContextChangeAttribute>(true);
            var dataContextManipulation = descriptorField.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
            if (dataContextManipulation != null && dataContextChange.Any()) throw new ArgumentException(
                $"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property '{name}'.");
            DataContextChangeAttributes = dataContextChange.ToArray();
            DataContextManipulationAttribute = dataContextManipulation;
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

        private static ConcurrentDictionary<string, DotvvmPropertyGroup > descriptorDictionary = new ConcurrentDictionary<string, DotvvmPropertyGroup >();

        public static DotvvmPropertyGroup  Create(PropertyInfo propertyInfo, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(propertyInfo.DeclaringType.Name + "." + propertyInfo.Name, fullName =>
            {
                var attribute = propertyInfo.GetCustomAttribute<PropertyGroupAttribute>();
                var valueType = attribute.ValueType ?? GetValueType(propertyInfo.PropertyType).valueType;
                return new DotvvmPropertyGroup (propertyInfo, attribute.Prefixes, valueType, defaultValue);
            });
        }

        public static DotvvmPropertyGroup  Register<TValue, TDeclaring>(PrefixArray prefixes, string name, TValue defaultValue = default(TValue)) =>
            Register(typeof(TDeclaring), prefixes, name, typeof(TValue), defaultValue);

        public static DotvvmPropertyGroup  Register(Type declaringType, PrefixArray prefixes, string name, Type valueType, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(declaringType.Name + "." + name, fullName =>
            {
                var fieldName = name + "GroupDescriptor";
                var field = declaringType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                if (field == null) throw new InvalidOperationException($"Could not declare property group '{fullName}' because backing field {fieldName} was not found.");
                return new DotvvmPropertyGroup (prefixes, valueType, field, name, defaultValue);
            });
        }

        public static IEnumerable<DotvvmPropertyGroup> FindAttachedPropertyCandidates(string typeName)
        {
            foreach (var pg in descriptorDictionary.Values)
            {
                if (pg.PropertyGroupMode == PropertyGroupMode.GeneratedDotvvmProperty && pg.DeclaringType.Name == typeName)
                {
                    yield return pg;
                }
            }
        }

        static void RunClassConstructor(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            if (type.BaseType != typeof(object))
                RunClassConstructor(type.BaseType);
        }

        public static IEnumerable<DotvvmPropertyGroup > GetPropertyGroups(Type controlType)
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

    public enum PropertyGroupMode: byte
    {
        /// <summary> Property group that is set into a real property with a Dictionary or so. Something like a virtual dotvvm property. </summary>
        ValueCollection,
        /// <summary> Properties are backend in DotvvmControl.properties and accessed through VirtualPropertyGroupDictionary </summary>
        GeneratedDotvvmProperty
    }
}
