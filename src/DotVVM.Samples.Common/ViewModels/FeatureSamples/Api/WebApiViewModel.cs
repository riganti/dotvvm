using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples;
using WebApiClient;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class WebApiViewModel : DotvvmViewModelBase
	{
        public IEnumerable<Country> Countries { get; set; }

	    public IEnumerable<Region> Region { get; set; } = null;

        public string RegName { get; set; }

        public Country SelectedCountry { get; set; }

	    public void SelectedCountryChanged()
	    {
	        //RegName = Region != null ? Region.Name : "Nothing selected";
	        RegName = "Changed";
	    }
	}
}

