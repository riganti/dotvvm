using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class NullValidationTargetViewModel : DotvvmViewModelBase
    {
        [Required]
        public SomeValidatebleObject NullObject { get; set; }
        [Required]
        public SomeValidatebleObject RealObject { get; set; } = new SomeValidatebleObject();
    }

    public class SomeValidatebleObject
    {
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Required { get; set; }
    }
}
