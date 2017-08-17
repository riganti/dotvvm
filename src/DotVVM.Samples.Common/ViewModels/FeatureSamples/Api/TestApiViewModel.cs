using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureFunctionsApi;
using DotVVM.Framework.ViewModel;


namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class TestApiViewModel : DotvvmViewModelBase
	{
        public string Text { get; set; } = "";

        public DataModel Data { get; set; } = new DataModel();
	    public DataModel PostResult { get; set; } = new DataModel();

        public string Json { get; set; }

	    public void ChangeText()
	    {
            Text += "a";
	    }
	}
}

