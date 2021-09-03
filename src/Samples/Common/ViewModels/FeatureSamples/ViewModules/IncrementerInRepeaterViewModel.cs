using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModules
{
    public class IncrementerInRepeaterViewModel : DotvvmViewModelBase
    {

        public List<int> Incrementers { get; set; } = new List<int>() {0, 0};

        public int ReportedState { get; set; }
        
    }
}

