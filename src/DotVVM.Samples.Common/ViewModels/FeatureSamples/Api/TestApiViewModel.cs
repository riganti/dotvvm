using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class TestApiViewModel : DotvvmViewModelBase
	{
        public string Text { get; set; } = "";

	    public void ChangeText()
	    {
	        Text += "a";
	    }
	}
}

