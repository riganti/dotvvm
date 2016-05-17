using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Resource
{
    public class OnlineNonameResourceLoad: DotvvmViewModelBase
    {
        public void Alert()
        {
            Context.ResourceManager.AddStartupScript("alert('resource loaded');");
        }
    }
}