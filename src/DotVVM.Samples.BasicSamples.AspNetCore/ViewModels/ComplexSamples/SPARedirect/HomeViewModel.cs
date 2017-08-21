using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Authentication;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize(ActiveAuthenticationSchemes = "Scheme2")]
    public class HomeViewModel : DotvvmViewModelBase
    {
        public async Task SignOut()
        {
            await Context.GetAspNetCoreContext().SignOutAsync("Scheme2");

            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", allowSpaRedirect: false);
        }
    }
}