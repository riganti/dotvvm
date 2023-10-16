using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Formatting
{
    public class ToStringGlobalFunctionBugViewModel : DotvvmViewModelBase
    {
        public bool Boolean { get; set; }

        public string String { get; set; }
    }
}

