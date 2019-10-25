using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.RouteLink
{
    public class RouteLinkQueryParametersViewModel : DotvvmViewModelBase
    {
        public int TestInteger { get; set; } = 5;
        public string TestString { get; set; } = "default";
    }
}

