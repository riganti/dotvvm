using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using TestApiClient;


namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class TestApiViewModel : DotvvmViewModelBase
	{
        public string Text { get; set; } = "";

        public Form Form { get; set; }

        public string Json { get; set; }

	    public TestApiViewModel()
	    {
	        Form = new Form()
	        {
	            Name = "Nove",
	            Number = 2
	        };
        }

	    public void ChangeText()
	    {
            Text += "a";
	    }
	}
}

