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

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class PropertyGroupDescriptor: IPropertyGroupDescriptor
    {
        public PropertyInfo PropertyInfo { get; }

        public FieldInfo DescriptorField { get; }

        public string Prefix { get; }

        public string PropertyName { get; }

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

        public bool CaseSensitive { get; }

        private ConcurrentDictionary<string, DotvvmProperty> generatedProperties = new ConcurrentDictionary<string, DotvvmProperty>();

        private PropertyGroupDescriptor(PropertyInfo propertyInfo, string prefix, Type valueType, object defaultValue)
        {
            this.PropertyInfo = propertyInfo;
            this.DeclaringType = propertyInfo.DeclaringType;
            this.CollectionType = propertyInfo.PropertyType;
            this.PropertyName = propertyInfo.Name;
            this.PropertyType = valueType;
            this.Prefix = prefix;
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

        private PropertyGroupDescriptor(string prefix, Type valueType, FieldInfo descriptorField, string name, object defaultValue)
        {
            this.PropertyInfo = null;
            this.DescriptorField = descriptorField;
            this.DeclaringType = descriptorField.DeclaringType;
            this.CollectionType = null;
            this.PropertyName = name;
            this.PropertyType = valueType;
            this.Prefix = prefix;
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

        public static Tuple<Type, MethodBase> GetValueType(Type declaringType)
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
            if (collectionCtors.Length > 1) throw new Exception(
                $"Could not initialize {declaringType.Name} as property group collections - constructors {string.Join(", ", collectionCtors.Select(c => c.ctor))} are ambitious.");
            if (collectionCtors.Length == 1) return new Tuple<Type, MethodBase>(collectionCtors[0].valueType, collectionCtors[0].ctor);

            throw new NotSupportedException($"Could not initialize {declaringType.Name} as proeprty group collection - no suitable constructor found");
        }

        private static ConcurrentDictionary<string, PropertyGroupDescriptor> descriptorDictionary = new ConcurrentDictionary<string, PropertyGroupDescriptor>();

        public static PropertyGroupDescriptor Create(PropertyInfo propertyInfo, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(propertyInfo.DeclaringType.Name + "," + propertyInfo.Name, fullName =>
            {
                var attribute = propertyInfo.GetCustomAttribute<PropertyGroupAttribute>();
                var valueType = attribute.ValueType ?? GetValueType(propertyInfo.PropertyType).Item1;
                return new PropertyGroupDescriptor(propertyInfo, attribute.Prefix, valueType, defaultValue);
            });
        }

        public static PropertyGroupDescriptor Create<TDeclaring, TValue>(string prefix, string name, TValue defaultValue = default(TValue)) =>
            Create(typeof(TDeclaring), prefix, name, typeof(TValue), defaultValue);
        public static PropertyGroupDescriptor Create(Type declaringType, string prefix, string name, Type valueType, object defaultValue)
        {
            return descriptorDictionary.GetOrAdd(declaringType.Name + "." + name, fullName =>
            {
                var field = declaringType.GetField(name + "GroupDescriptor", BindingFlags.Public | BindingFlags.Static);
                if (field == null) throw new InvalidOperationException($"Could not declare property group '{fullName}' because backing field was not found.");
                return new PropertyGroupDescriptor(prefix, valueType, field, name, defaultValue);
            });
        }

        public static IEnumerable<PropertyGroupDescriptor> FindAttachedPropertyCandidates(string typeName)
        {
            foreach (var pg in descriptorDictionary.Values)
            {
                if (pg.PropertyGroupMode == PropertyGroupMode.GeneratedDotvvmProperty && pg.DeclaringType.Name == typeName)
                {
                    yield return pg;
                }
            }
        }

        public static IEnumerable<PropertyGroupDescriptor> GetPropertyGroups(Type controlType)
        {
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
    }

    public enum PropertyGroupMode: byte
    {
        ValueCollection,
        GeneratedDotvvmProperty
    }
}
