using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample10ViewModel : RedwoodViewModelBase
    {
        public override Task Init()
        {
            if (string.IsNullOrEmpty(Context.Query["time"]))
            {
                Context.Redirect("~/Sample10?time=" + DateTime.Now.Ticks);
            }

            return base.Init();
        }

        public void RedirectTest()
        {
            Context.Redirect("~/Sample10?time=" + DateTime.Now.Ticks);

            throw new Exception("This exception should not occur because Redirect interrupts the request execution!");
        }
    }
}