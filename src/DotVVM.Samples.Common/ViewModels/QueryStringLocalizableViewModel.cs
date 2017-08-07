using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class QueryStringLocalizableViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            var value = Context.Query.ContainsKey("lang") ? Context.Query["lang"] : "";
            Context.ChangeCurrentCulture(string.IsNullOrWhiteSpace(value) ? "en-US" : value);
            return base.Init();
        }
    }
}