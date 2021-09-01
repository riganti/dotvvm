using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureFunctionsApi;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class AzureFunctionsApiTableViewModel : DotvvmViewModelBase
    {
        public CreateFormModel NewForm { get; set; } = new CreateFormModel { Date = DateTime.Now };
    }
}

