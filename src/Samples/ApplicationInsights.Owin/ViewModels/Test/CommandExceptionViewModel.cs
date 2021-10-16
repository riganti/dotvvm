using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.ApplicationInsights.Owin.ViewModels.Test
{
    public class CommandExceptionViewModel : DotvvmViewModelBase
    {
        public void TestCommand()
        {
            throw new ArgumentException("Throwing exception in button command");
        }
    }
}
