using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostbackConcurrency
{
    public class RedirectPostbackQueueViewModel : RedirectPostbackQueueMasterViewModel
    {

        public int Value { get; set; }

        public override async Task Init()
        {
            if (!Context.IsPostBack && Context.Query.ContainsKey("time"))
            {
                await Task.Delay(5000);
            }

            await base.Init();
        }

        public void Increment()
        {
            Value++;
        }

        public void IncrementWithWait()
        {
            Thread.Sleep(5000);
            Value++;
        }

        public void Redirect()
        {
            Thread.Sleep(2000);
            Context.RedirectToRoute(Context.Route.RouteName, query: new { time = Environment.TickCount });
        }

    }
}
