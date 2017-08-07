using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Localization
{
    public class LocalizationNestedViewModel : MasterPageViewModel
    {
        public override Task Init()
        {
            if (Context.Query.ContainsKey("lang") && Context.Query["lang"] == "cs-CZ")
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