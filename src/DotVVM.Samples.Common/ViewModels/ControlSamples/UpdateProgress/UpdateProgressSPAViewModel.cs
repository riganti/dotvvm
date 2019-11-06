using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressSPAViewModel : DotvvmViewModelBase
    {

        public async Task Redirect2()
        {
            await Task.Delay(2000);

            Context.RedirectToRoute("ControlSamples_UpdateProgress_UpdateProgressSPA2");
        }

        public async Task Redirect1()
        {
            await Task.Delay(2000);

            Context.RedirectToRoute("ControlSamples_UpdateProgress_UpdateProgressSPA1");
        }

    }
}

