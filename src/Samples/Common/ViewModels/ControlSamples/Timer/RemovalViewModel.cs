using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Timer
{
    public class RemovalViewModel : DotvvmViewModelBase
    {
        public bool Disabled { get; set; } = false;

        public int Value { get; set; }

        public bool IsRemoved { get; set; } = false;

    }
}

