using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ActionFilterErrorHandling
{
    [ActionFilterRedirectExceptionFilter]
    public class ActionFilterRedirectViewModel : DotvvmViewModelBase
    {
        private bool throwOnPreRender;

        public void TestCommandException()
        {
            throw new Exception("Exception");
        }

        public void TestPageException()
        {
            throwOnPreRender = true;
        }

        public override Task PreRender()
        {
            if (throwOnPreRender)
            {
                throw new Exception("Exception");
            }

            return base.PreRender();
        }

    }

    public class ActionFilterRedirectExceptionFilterAttribute : ExceptionFilterAttribute
    {
        protected override Task OnCommandExceptionAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
        {
            if (!context.HttpContext.Request.Query.ContainsKey("redirected"))
            {
                context.RedirectToUrl(context.HttpContext.Request.Url.AbsoluteUri + "?redirected=true");
            }

            return base.OnPageExceptionAsync(context, exception);
        }

        protected override Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
        {
            if (!context.HttpContext.Request.Query.ContainsKey("redirected"))
            {
                context.RedirectToUrl(context.HttpContext.Request.Url.AbsoluteUri + "?redirected=true");
            }

            return base.OnPageExceptionAsync(context, exception);
        }
    }
}
