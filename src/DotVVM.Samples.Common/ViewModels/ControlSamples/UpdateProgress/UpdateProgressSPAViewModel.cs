using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressSPAViewModel : DotvvmViewModelBase
    {

        public void Redirect2()
        {
            Context.RedirectToRoute("ControlSamples_UpdateProgress_UpdateProgressSPA2");
        }

        public void Redirect1()
        {
            Context.RedirectToRoute("ControlSamples_UpdateProgress_UpdateProgressSPA1");
        }
    }
}

