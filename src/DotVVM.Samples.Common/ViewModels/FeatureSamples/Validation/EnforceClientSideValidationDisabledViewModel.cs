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
        [DotvvmClientFormat(Disable = true, ErrorMessage = "This message will not be displayed. Because this client format enforcing is disabled.")]
        public int? NullableIntegerProperty { get; set; }

        [DotvvmClientFormat(Disable = true, ErrorMessage = "This message will not be displayed. Because this client format enforcing is disabled.")]
        public DateTime? NullableDateTimeProperty { get; set; }

        public DateTime DateTimeProperty { get; set; } = DateTime.Now;
        [Required (ErrorMessage = "Int requires value.")]
        public int IntegerProperty { get; set; }
        public void Postback()
        {
        }
    }       
}

