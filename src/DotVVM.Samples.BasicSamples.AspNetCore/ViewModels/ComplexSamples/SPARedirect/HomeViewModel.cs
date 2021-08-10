using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize(AuthenticationSchemes = "Scheme2")]
    public class HomeViewModel : DotvvmViewModelBase
    {
        public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync("Scheme2");

            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", allowSpaRedirect: false);
        }
    }
}