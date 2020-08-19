using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBackSpaNavigation
{
    public class PageAViewModel : DenyPostbacksOnSpaNavigationViewModel
    {

        public int Result { get; set; }

        public void Command()
        {
            Result++;
        }

    }
}