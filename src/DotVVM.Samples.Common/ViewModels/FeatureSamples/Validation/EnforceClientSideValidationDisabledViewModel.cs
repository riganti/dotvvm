using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
	public class EnforceClientSideValidationDisabledViewModel
    {
        [DotvvmEnforceClientFormat(Enforce = false, ErrorMessage = "This message will not be display. Because this client format enforcing is disabled.")]
        public int? NullableIntegerProperty { get; set; }
        [Required (ErrorMessage = "Int requires value.")]
        public int IntegerProperty { get; set; }
        public void Postback()
        {
        }
    }       
}

