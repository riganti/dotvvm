using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.ApplicationInsights.Owin.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        public string InitRouteName { get; set; }
        public string CommandRouteName { get; set; }


        public DefaultViewModel()
        {
            InitRouteName = "InitException";
            CommandRouteName = "CommandException";
        }
    }
}
