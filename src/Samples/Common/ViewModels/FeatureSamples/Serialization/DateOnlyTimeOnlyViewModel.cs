using System;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class DateOnlyTimeOnlyViewModel : DotvvmViewModelBase
    {
        public DateOnly DateOnly { get; set; } = new DateOnly(2022, 9, 14);
        public TimeOnly TimeOnly { get; set; } = new TimeOnly(23, 56, 42, 123);
    }
}

