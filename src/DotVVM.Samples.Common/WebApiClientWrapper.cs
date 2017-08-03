using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Samples.BasicSamples
{
    public class WebApiClientWrapper
    {
        public WebApiClient.ValuesClient ValuesClient { get; set; }

        public WebApiClient.CountriesClient CountriesClient { get; set; }
    }
}
