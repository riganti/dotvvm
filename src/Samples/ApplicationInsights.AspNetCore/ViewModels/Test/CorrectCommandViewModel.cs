using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.ApplicationInsights.AspNetCore.ViewModels.Test
{
    public class CorrectCommandViewModel : DotvvmViewModelBase
    {
        public void Submit()
        {
        }
        public void Redirect()
        {
            Context.RedirectToRoute("default");
        }
    }
}
