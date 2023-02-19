using System;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    [CustomPrimitiveType(typeof(Guid?), typeof(TypeIdPrimitiveTypeConverter<SampleId>))]
    public record SampleId : TypeId<SampleId>
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }

        public override string ToString() => base.ToString();
    }
}

