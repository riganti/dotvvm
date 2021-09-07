using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.ApplicationInsights.Owin.ViewModels.Test
{
    public class InitExceptionViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            throw new ArgumentException("Throwing exception in init phase of the request");
        }
    }
}
