using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBackSpaNavigation
{
    public class PageBViewModel : DenyPostbacksOnSpaNavigationViewModel
    {
        public override Task Init()
        {
            Thread.Sleep(2000);

            if (Context.Query.ContainsKey("fail"))
            {
                throw new DotvvmHttpException("Intentional error.");
            }

            return base.Init();
        }
    }
}