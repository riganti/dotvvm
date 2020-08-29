using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
