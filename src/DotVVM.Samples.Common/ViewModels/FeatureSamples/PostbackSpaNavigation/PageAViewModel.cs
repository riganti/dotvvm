using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBackSpaNavigation
{
    public class PageAViewModel : DenyPostbacksOnSpaNavigationViewModel
    {

        public int Result { get; set; }

        public void Command()
        {
            Result++;
        }

        public void LongCommand()
        {
            Thread.Sleep(5000);
            Result++;
        }
    }
}
