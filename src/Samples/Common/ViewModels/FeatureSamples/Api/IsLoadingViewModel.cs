using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class IsLoadingViewModel : DotvvmViewModelBase
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
        public int Number3 { get; set; }
    }
}

