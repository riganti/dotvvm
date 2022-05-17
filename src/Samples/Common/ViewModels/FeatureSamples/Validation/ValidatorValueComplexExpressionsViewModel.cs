using System;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ValidatorValueComplexExpressionsViewModel : DotvvmViewModelBase
    {
        [Required]
        public DateTime DateTime { get; set; }
    }
}

