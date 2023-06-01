using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class RangeClientSideValidationViewModel : DotvvmViewModelBase
    {
        [Range(1, 999, ErrorMessage = "Invalid number")]
        public int? NullableInt { get; set; }

        public string Result { get; set; }

        public void Process()
        {
            Result = "Valid";
        }
    }
}

