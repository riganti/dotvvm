using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressDelayViewModel : DotvvmViewModelBase
    {
        public void LongTest()
        {
            Thread.Sleep(3000);
        }

        public void ShortTest()
        {
            Thread.Sleep(1000);
        }
    }
}
