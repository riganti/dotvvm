using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.MarkupControl
{
	public class ControlPropertyValidationPageViewModel : DotvvmViewModelBase
	{
        [Required]
        [EmailAddress]
        public string Text { get; set; }

        public void Postback()
        {

        }
    }
}

