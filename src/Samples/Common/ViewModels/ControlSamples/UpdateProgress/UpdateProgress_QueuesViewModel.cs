using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressQueuesViewModel : DotvvmViewModelBase
    {

        public int Counter { get; set; }


        public void Test()
        {
            Thread.Sleep(1000);
            Counter++;
        }

    }
}

