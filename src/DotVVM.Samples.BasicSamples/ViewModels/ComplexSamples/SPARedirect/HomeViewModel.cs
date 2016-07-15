using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize(AuthScheme = "Scheme2")]
	public class HomeViewModel : DotvvmViewModelBase
	{

        public async Task SignOut()
        {
            await Context.HttpContext.Authentication.SignOutAsync("Scheme2");
            
            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", forceRefresh: true);
        }

	}
}

