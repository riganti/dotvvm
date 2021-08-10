using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
    [Authorize()]
	public class HomeViewModel : DotvvmViewModelBase
	{

        public void SignOut()
        {
            Context.GetAuthentication().SignOut("ApplicationCookie");
            
            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", allowSpaRedirect: false);
        }

	}
}

