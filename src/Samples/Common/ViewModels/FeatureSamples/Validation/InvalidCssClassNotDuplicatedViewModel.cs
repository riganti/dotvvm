using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class InvalidCssClassNotDuplicatedViewModel : DotvvmViewModelBase
    {

        [Required]
        public int? Value { get; set; }


        public void Validate()
        {
            
        }

    }
}

