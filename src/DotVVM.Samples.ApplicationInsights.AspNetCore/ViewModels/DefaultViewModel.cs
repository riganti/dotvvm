using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.ApplicationInsights.AspNetCore.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        
        public string Title { get; set; }


        public DefaultViewModel()
        {
            Title = "Hello from DotVVM!";
        }
    }
}
