using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples
{
    public class RedirectViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            if (!Context.Query.ContainsKey("time"))
            {
                Context.Redirect("~/Redirect?time=" + DateTime.Now.Ticks);
            }

            return base.Init();
        }

        public void RedirectTest()
        {
            Context.Redirect("~/Redirect?time=" + DateTime.Now.Ticks);

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
    }
}