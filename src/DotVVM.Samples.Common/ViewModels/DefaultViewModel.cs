using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        public string Title { get; set; }
        public List<RouteBase> Routes { get; set; }

        public override Task Init()
        {
            Routes = new List<RouteBase>(Context.Configuration.RouteTable);
            return base.Init();
        }
    }
}