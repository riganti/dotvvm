using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;
using DotVVM.Samples.BasicSamples.Utilities;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.MarkupControl
{
	public class ControlPropertyValidationPageViewModel : DotvvmViewModelBase
	{
        [Required]
        [OnlyServerSideEmailAddress]
        public string Text { get; set; }

        public void Postback()
        {

        }
    }
}

