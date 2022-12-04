using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ActionFilterErrorHandling
{
    [PageErrorHandlingFilter]
    public class ActionFilterPageErrorHandlingViewModel : DotvvmViewModelBase
    {
        public override Task Load()
        {
            throw new Exception("Error from the viewmodel event.");
        }
    }

    public class PageErrorHandlingFilterAttribute : ActionFilterAttribute
    {
        protected override Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
        {
            context.IsPageExceptionHandled = true;
            context.RedirectToUrl("/error500");
            return Task.CompletedTask;
        }
    }
}
