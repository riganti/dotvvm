using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
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

        public void RedirectObjectQueryString()
        {
            Context.RedirectToRoute("FeatureSamples_Redirect_Redirect", urlSuffix: "?param=temp1#test1", query: new { time = DateTime.Now.Ticks });

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }

        public void RedirectDictionaryQueryString()
        {
            Context.RedirectToRoute("FeatureSamples_Redirect_Redirect", urlSuffix: "#test2", query: new Dictionary<string, string> { { "time", DateTime.Now.Ticks.ToString() }, { "param", "temp2" } });

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
    }
}
