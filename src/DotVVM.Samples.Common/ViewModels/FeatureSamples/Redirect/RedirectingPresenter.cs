using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect
{
    public class RedirectingPresenter : IDotvvmPresenter
    {
        public Task ProcessRequest(IDotvvmRequestContext context)
        {
            context.RedirectToRoute("FeatureSamples_Redirect_Redirect");

            throw new Exception("This should never happen!");
        }
    }
}