#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    public partial class DotvvmCapabilityProperty
    {
        class CombinedDataContextAttributeProvider : ICustomAttributeProvider
        {
            DataContextChangeAttribute[] baseAttributes;
            DataContextStackManipulationAttribute? baseManipulationAttribute;
            ICustomAttributeProvider provider;

            public CombinedDataContextAttributeProvider(DataContextChangeAttribute[] baseAttributes, DataContextStackManipulationAttribute? baseManipulationAttribute, ICustomAttributeProvider provider)
            {
                this.baseAttributes = baseAttributes;
                this.baseManipulationAttribute = baseManipulationAttribute;
                this.provider = provider;
            }

            public static ICustomAttributeProvider Create(ICustomAttributeProvider? baseProvider, ICustomAttributeProvider provider)
            {
                if (baseProvider is null) 
                    return provider;
                var baseAttributes = baseProvider.GetCustomAttributes<DataContextChangeAttribute>().ToArray();
                var baseManipulationAttribute = baseProvider.GetCustomAttribute<DataContextStackManipulationAttribute>();
                if (baseAttributes.Any() || baseManipulationAttribute is not null)
                    return new CombinedDataContextAttributeProvider(baseAttributes, baseManipulationAttribute, provider);
                return provider;
            }

            public object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                var realAttributes = provider.GetCustomAttributes(attributeType, inherit);
                return GatherAttributes(realAttributes).Where(x => x is object && attributeType.IsInstanceOfType(x)).ToArray()!;
            }

            public object[] GetCustomAttributes(bool inherit)
            {
                var realAttributes = provider.GetCustomAttributes(inherit);
                return GatherAttributes(realAttributes).OfType<object>().ToArray();
            }

            IEnumerable<object?> GatherAttributes(object[] realAttributes)
            {
                if (realAttributes.OfType<DataContextStackManipulationAttribute>().Any() || realAttributes.OfType<DataContextChangeAttribute>().Any())
                {
                    throw new NotSupportedException($"Capability property is annotated with data context change attributes and a property inside of it has some too. This is currently not supported.");
                }
                return realAttributes.Concat(baseAttributes).Concat(new[] { baseManipulationAttribute }).ToArray();
            }

            public bool IsDefined(Type attributeType, bool inherit) =>
                provider.IsDefined(attributeType, inherit) ||
                (typeof(DataContextChangeAttribute).IsAssignableFrom(attributeType) && baseAttributes.Any()) ||
                (typeof(DataContextStackManipulationAttribute).IsAssignableFrom(attributeType) && baseManipulationAttribute is not null);
        }

	    class CustomAttributesProvider : ICustomAttributeProvider
        {
            private readonly object[] attributes;
            public CustomAttributesProvider(params object[] attributes)
            {
                this.attributes = attributes;
            }
            public object[] GetCustomAttributes(bool inherit) => attributes;

            public object[] GetCustomAttributes(Type attributeType, bool inherit) => GetCustomAttributes(inherit).Where(attributeType.IsInstanceOfType).ToArray();

            public bool IsDefined(Type attributeType, bool inherit) => GetCustomAttributes(attributeType, inherit).Any();
        }

    }
}
