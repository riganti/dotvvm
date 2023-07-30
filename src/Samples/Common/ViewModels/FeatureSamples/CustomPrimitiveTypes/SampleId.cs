using System;
using System.Security.Cryptography;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    [CustomPrimitiveType]
    public record SampleId : TypeId<SampleId>
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }
    }
}

