using System;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect
{
    public class RedirectViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            if (!Context.Query.ContainsKey("time"))
            {
                Context.RedirectToUrl("~/FeatureSamples/Redirect/Redirect?time=" + DateTime.Now.Ticks);
            }

            return base.Init();
        }

        public void RedirectTest()
        {
            Context.RedirectToUrl("~/FeatureSamples/Redirect/Redirect?time=" + DateTime.Now.Ticks);

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
    }
}