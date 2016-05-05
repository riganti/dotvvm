using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ActionFilterErrorHandling
{
	public class ActionFilterErrorHandlingViewModel : DotvvmViewModelBase
	{

	    public string Result { get; set; } = "no error";

        [ErrorHandlingActionFilter]
	    public void HandledError()
	    {
	        throw new Exception("Error in command!");
	    }

        public void Error()
        {
            throw new Exception("Error in command!");
        }

    }

    public class ErrorHandlingActionFilter : ExceptionFilterAttribute
    {
        protected override void OnCommandException(IDotvvmRequestContext context, ActionInfo actionInfo, Exception ex)
        {
            ((ActionFilterErrorHandlingViewModel) context.ViewModel).Result = "error was handled";
            context.IsCommandExceptionHandled = true;

            base.OnCommandException(context, actionInfo, ex);
        }
    }
}

