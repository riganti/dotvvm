using DotVVM.Framework.ViewModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class DateTimeValidationViewModel : DotvvmViewModelBase
    {
        [DotvvmClientFormat(Disable = true)]
        public DateTime? Value1 { get; set; }

        public DateTime? Value2 { get; set; }

        [Required]
        public DateTime? Value3 { get; set; }

        public DateTime Value4 { get; set; }

        [Required]
        public DateTime Value5 { get; set; }

        public void ValidateRequiredDateTime()
        {
        }

    }
}
