using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class NestedValidationViewModel : DotvvmViewModelBase
    {

        public NestedValidationChildViewModel Child { get; set; }

        public NestedValidationViewModel()
        {
            Child = new NestedValidationChildViewModel();
        }

        public void Test()
        {

        }
    }

    public class NestedValidationChildViewModel
    {
        [Required]
        public string Text { get; set; }
    }
}