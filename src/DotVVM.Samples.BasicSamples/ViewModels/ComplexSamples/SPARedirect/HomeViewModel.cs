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

            var url = Context.Configuration.RouteTable["ComplexSamples_SPARedirect_home"].BuildUrl(new { });
            Context.RedirectToUrl(url + "?refresh=1");
        }

	}
}

