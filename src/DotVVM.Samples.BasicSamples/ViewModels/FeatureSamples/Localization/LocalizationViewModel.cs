using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Localization
{
    public class LocalizationViewModel : DotvvmViewModelBase
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
