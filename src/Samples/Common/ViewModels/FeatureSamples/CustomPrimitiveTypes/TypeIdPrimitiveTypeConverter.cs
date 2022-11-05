using DotVVM.Framework.Configuration;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes;

public class TypeIdPrimitiveTypeConverter<TId> : ICustomPrimitiveTypeConverter
    where TId : TypeId<TId>
{
    public object Convert(object value) => TypeId<TId>.ParseValue(value);
}
