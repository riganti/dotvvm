using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample9ViewModel : RedwoodViewModelBase
    {
        public override Task Init()
        {
            var lang = Context.Query["lang"];
            if (lang == "cs-CZ")
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