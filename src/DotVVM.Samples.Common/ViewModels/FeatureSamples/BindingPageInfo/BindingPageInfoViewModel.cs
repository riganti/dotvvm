using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.BindingPageInfo
{
    public class BindingPageInfoViewModel
    {
        public void LongCommand() => Thread.Sleep(1000);
    }
}