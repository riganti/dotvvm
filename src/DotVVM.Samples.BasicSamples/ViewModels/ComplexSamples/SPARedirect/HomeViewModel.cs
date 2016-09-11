using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize]
	public class HomeViewModel : DotvvmViewModelBase
	{

        public void SignOut()
        {
            Context.OwinContext.Authentication.SignOut();
            
            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", allowSpaRedirect: false);
        }

	}
}

