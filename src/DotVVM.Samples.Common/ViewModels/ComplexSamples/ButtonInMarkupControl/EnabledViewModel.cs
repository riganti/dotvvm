using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ButtonInMarkupControl
{
    public class EnabledViewModel
    {
        public bool Enabled { get; set; }

        public Task Flip()
        {
            Enabled = !Enabled;
            return Task.CompletedTask;
        }
    }
}
