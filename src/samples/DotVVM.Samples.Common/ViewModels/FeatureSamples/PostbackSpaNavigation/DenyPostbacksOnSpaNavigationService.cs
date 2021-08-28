using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBackSpaNavigation
{
    public class DenyPostbacksOnSpaNavigationService
    {
        [AllowStaticCommand]
        public int StaticCommand(int result)
        {
            return result + 1;
        }

        [AllowStaticCommand]
        public int LongStaticCommand(int result)
        {
            Thread.Sleep(5000);
            return result + 1;
        }

    }
}
