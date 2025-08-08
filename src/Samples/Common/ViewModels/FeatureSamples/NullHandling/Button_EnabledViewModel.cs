using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.NullHandling
{
    public class Button_EnabledViewModel : DotvvmViewModelBase
    {

        public NullObject Null { get; set; } = null;

        public int Value { get; set; }

        public class NullObject
        {
            public bool Enabled { get; set; }
        }
    }

}

