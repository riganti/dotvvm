using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class QueryStringLocalizableViewModel : DotvvmViewModelBase
    {
        public QueryStringLocalizableViewModel()
        {
        }

        public override Task Init()
        {
            var value = Context.Query.ContainsKey("lang") ? Context.Query["lang"].ToString() : "";
            if (string.IsNullOrWhiteSpace(value))
            {
                Context.ChangeCurrentCulture("en-US");
            }
            else
            {
                Context.ChangeCurrentCulture(value);
            }
            return base.Init();
        }
    }
}