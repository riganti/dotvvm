using System;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public record SampleId : TypeId<SampleId>
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }
    }

}

