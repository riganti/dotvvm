using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ActionFilterErrorHandling
{
    [PageErrorHandlingFilter]
	public class ActionFilterPageErrorHandlingViewModel : DotvvmViewModelBase
	{
        public override Task Load()
        {
            throw new Exception("Error from the viewmodel event.");
            return base.Load();
        }
	}

    public class PageErrorHandlingFilterAttribute : ActionFilterAttribute 
    {
        protected override void OnPageException(IDotvvmRequestContext context, Exception exception)
        {
            context.IsPageExceptionHandled = true;
            context.RedirectToUrl("/error500");

            base.OnPageException(context, exception);
        }
    }
}

