using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes;

public class TypeIdPrimitiveTypeConverter<TId> : ICustomPrimitiveTypeConverter
    where TId : TypeId<TId>
{
    public object FromCustomPrimitiveType(object value) => (value as TypeId<TId>)?.IdValue;
    public object ToCustomPrimitiveType(object value) => TypeId<TId>.ParseValue(value);
}
