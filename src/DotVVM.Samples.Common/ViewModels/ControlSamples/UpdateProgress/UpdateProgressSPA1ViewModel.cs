using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.UpdateProgress
{
    public class UpdateProgressSPA1ViewModel : UpdateProgressSPAViewModel
    {
        public async override Task PreRender()
        {
            await Task.Delay(2000);
            await base.PreRender();
        }
        public async Task LongTest()
        {
            await Task.Delay(5000);
        }
    }
}

