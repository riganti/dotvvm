using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA
{
	public class SiteViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.SignData)]
        public string ProtectedValue { get; set; }

        public string SampleText { get; set; }
        public void AddSampleText()
        {
            SampleText = "Sample Text";
            ProtectedValue = "protected value";
        }

        [AllowStaticCommand]
        public static string GetSampleText() => "Sample Static Text";
    }
}

