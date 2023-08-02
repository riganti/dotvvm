using System;
using System.Security.Cryptography;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public record SampleId : TypeId<SampleId>, IDotvvmPrimitiveType
    {
        public SampleId(Guid idValue) : base(idValue)
        {
        }
    }
}

