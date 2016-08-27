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

		public Type DeclaringType { get; }
		ITypeDescriptor IControlAttributeDescriptor.DeclaringType => new ResolvedTypeDescriptor(DeclaringType);

		public Type PropertyType { get; }
		ITypeDescriptor IControlAttributeDescriptor.PropertyType => new ResolvedTypeDescriptor(PropertyType);

		public Type CollectionType { get; }
		ITypeDescriptor IPropertyGroupDescriptor.CollectionType => new ResolvedTypeDescriptor(CollectionType);

		public PropertyGroupMode PropertyGroupMode { get; }

		private PropertyGroupDescriptor(PropertyInfo propertyInfo, string prefix, Type valueType)
		{
			this.PropertyInfo = propertyInfo;
			this.DeclaringType = propertyInfo.DeclaringType;
			this.CollectionType = propertyInfo.PropertyType;
			this.PropertyName = propertyInfo.Name;
			this.PropertyType = valueType;
			this.Prefix = prefix;
			this.PropertyGroupMode = PropertyGroupMode.ValueCollection;

			var markupOptions = this.MarkupOptions = propertyInfo.GetCustomAttribute<MarkupOptionsAttribute>(true) ?? new MarkupOptionsAttribute();
			var dataContextChange = propertyInfo.GetCustomAttributes<DataContextChangeAttribute>(true);
			var dataContextManipulation = propertyInfo.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
			if (dataContextManipulation != null && dataContextChange.Any()) throw new ArgumentException($"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property '{propertyInfo.Name}'.");
			DataContextChangeAttributes = dataContextChange.ToArray();
			DataContextManipulationAttribute = dataContextManipulation;
		}

		private PropertyGroupDescriptor(string prefix, Type valueType, FieldInfo descriptorField, string name)
		{
			this.PropertyInfo = null;
			this.DeclaringType = descriptorField.DeclaringType;
			this.CollectionType = null;
			this.PropertyName = name;
			this.PropertyType = valueType;
			this.Prefix = prefix;
			this.PropertyGroupMode = PropertyGroupMode.ValueCollection;

			var markupOptions = this.MarkupOptions = descriptorField.GetCustomAttribute<MarkupOptionsAttribute>(true) ?? new MarkupOptionsAttribute();
			var dataContextChange = descriptorField.GetCustomAttributes<DataContextChangeAttribute>(true);
			var dataContextManipulation = descriptorField.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
			if (dataContextManipulation != null && dataContextChange.Any()) throw new ArgumentException($"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property '{name}'.");
			DataContextChangeAttributes = dataContextChange.ToArray();
			DataContextManipulationAttribute = dataContextManipulation;
		}

		public static Tuple<Type, MethodBase> GetValueType(Type declaringType)
		{
			var collectionCtors = (from ctor in declaringType.GetConstructors()
								  let parameters = ctor.GetParameters()
								  where parameters.Length == 1 && typeof(IEnumerable).IsAssignableFrom(parameters[0].ParameterType)
								  let elementType = ReflectionUtils.GetEnumerableType(parameters[0].ParameterType)
								  where elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
								  let genArguments = elementType.GetGenericArguments()
								  where genArguments[0].IsAssignableFrom(typeof(string))
								  let valueType = genArguments[1]
								  select new { ctor, parameters, valueType }).ToArray();
			if (collectionCtors.Length > 1) throw new Exception($"Could not initialize {declaringType.Name} as property group collections - constructors {string.Join(", ", collectionCtors.Select(c => c.ctor))} are ambitious.");
			if (collectionCtors.Length == 1) return new Tuple<Type, MethodBase>(collectionCtors[0].valueType, collectionCtors[0].ctor);

			throw new NotSupportedException($"Could not initialize {declaringType.Name} as proeprty group collection - no suitable constructor found");
		}

		private static ConcurrentDictionary<string, PropertyGroupDescriptor> descriptorDictionary = new ConcurrentDictionary<string, PropertyGroupDescriptor>();

		public static PropertyGroupDescriptor Create(PropertyInfo propertyInfo)
		{
			return descriptorDictionary.GetOrAdd(propertyInfo.DeclaringType.Name + "," + propertyInfo.Name, fullName =>
			{
				var attribute = propertyInfo.GetCustomAttribute<PropertyGroupAttribute>();
				var valueType = GetValueType(propertyInfo.PropertyType).Item1;
				return new PropertyGroupDescriptor(propertyInfo, attribute.Prefix, valueType);
			});
		}

		public static PropertyGroupDescriptor Create(Type declaringType, string name)
		{
			return descriptorDictionary.GetOrAdd(declaringType.Name + "." + name, fullName =>
			{
				var field = declaringType.GetField(name + "Property", BindingFlags.Public | BindingFlags.Static);
				if (field == null) throw new InvalidOperationException($"Could not declare property group '{fullName}' because backing field was not found.");
				var valueType = GetValueType(propertyInfo.PropertyType).Item1;
				return new PropertyGroupDescriptor(propertyInfo, attribute.Prefix, valueType);
			});
		}

		public static IEnumerable<PropertyGroupDescriptor> GetPropertyGroups(Type controlType)
		{
			foreach (var property in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
			{
				if (property.IsDefined(typeof(PropertyGroupAttribute)))
				{
					yield return Create(property);
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
