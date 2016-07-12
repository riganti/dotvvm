using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize]
	public class HomeViewModel : DotvvmViewModelBase
	{

        public async Task SignOut()
        {
            await Context.HttpContext.Authentication.SignOutAsync("Cookie");
            
            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", forceRefresh: true);
        }

	}
}

