using System;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    [CustomPrimitiveType(typeof(Guid?), typeof(TypeIdPrimitiveTypeConverter<SampleId>))]
    [JsonConverter(typeof(TypeIdJsonConverter<SampleId>))]
    public record SampleId : TypeId<SampleId>
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }
    }
}

