using DotVVM.Framework.ViewModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class NullableDateTimeValidationViewModel : DotvvmViewModelBase
    {
        [DotvvmClientFormat]
        public DateTime? DateTimeTestValue { get; set; } = DateTime.Now;

        public bool DateTimeTestResult { get; set; }

        public void ValidateNullableDateTime()
        {
            DateTimeTestResult = true;
        }
    }
}
