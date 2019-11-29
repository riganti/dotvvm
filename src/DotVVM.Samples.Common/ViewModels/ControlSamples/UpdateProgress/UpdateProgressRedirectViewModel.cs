using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressRedirectViewModel : UpdateProgressRedirectMasterPageViewModel
    {
        public async override Task PreRender()
        {
            await base.PreRender();
        }
        public async Task LongTest()
        {
            await Task.Delay(1000);
        }
    }
}

