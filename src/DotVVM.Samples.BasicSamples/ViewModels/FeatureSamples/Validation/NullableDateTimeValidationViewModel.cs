using DotVVM.Framework.ViewModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class NullableDateTimeValidationViewModel : DotvvmViewModelBase
    {

        [DotvvmEnforceClientFormat]
        public DateTime? DateTimeTestValue { get; set; } = DateTime.Now;
        
        public bool DateTimeTestResult { get; set; }

        private DateTime defaultValue = new DateTime(2016, 3, 1);


        public void ValidateNullableDateTime()
        {
        }
    }
}