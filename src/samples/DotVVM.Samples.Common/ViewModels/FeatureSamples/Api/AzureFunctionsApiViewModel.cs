using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureFunctionsApi;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class AzureFunctionsApiViewModel : DotvvmViewModelBase
    {
        public DataModel Data { get; set; } = new DataModel();
        public DataModel PostResult { get; set; } = new DataModel();
        public Country Country { get; set; } = new Country();
    }
}

