using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA
{
	public class SiteViewModel : DotvvmViewModelBase
	{
        public string SampleText { get; set; }
        public void AddSampleText()
        {
            SampleText = "Sample Text";
        }

        [AllowStaticCommand]
        public static string GetSampleText() => "Sample Static Text";
    }
}

