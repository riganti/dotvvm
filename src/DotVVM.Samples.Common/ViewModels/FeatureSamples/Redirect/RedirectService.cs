using System;
using System.Collections.Generic;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect
{
    public class RedirectService
    {
        public IDotvvmRequestContext Context { get; }

        public RedirectService(IDotvvmRequestContext context)
        {
            Context = context;
        }
        [AllowStaticCommand]
        public void RedirectTest()
        {
            Context.RedirectToUrl("~/FeatureSamples/Redirect/Redirect_StaticCommand?time=" + DateTime.Now.Ticks);

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
        [AllowStaticCommand]
        public void RedirectObjectQueryString()
        {
            Context.RedirectToRoute("FeatureSamples_Redirect_Redirect_StaticCommand", urlSuffix: "?param=temp1#test1", query: new { time = DateTime.Now.Ticks });

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
        [AllowStaticCommand]
        public void RedirectDictionaryQueryString()
        {
            Context.RedirectToRoute("FeatureSamples_Redirect_Redirect_StaticCommand", urlSuffix: "#test2", query: new Dictionary<string, string> { { "time", DateTime.Now.Ticks.ToString() }, { "param", "temp2" } });

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
    }
}
