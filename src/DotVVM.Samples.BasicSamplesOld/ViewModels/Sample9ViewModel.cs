using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample9ViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            if (Context.Query.ContainsKey("lang") && Context.Query["lang"] as string == "cs-CZ")
            {
                Context.ChangeCurrentCulture("cs-CZ");
            }
            else
            {
                Context.ChangeCurrentCulture("en-US");
            }
            
            return base.Init();
        }
    }
}