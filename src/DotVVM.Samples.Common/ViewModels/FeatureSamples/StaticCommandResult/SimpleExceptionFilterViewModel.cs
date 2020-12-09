using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommandResult
{
    public class PageErrorModel
    {
        public string Message { get; set; }
    }
    public class ClientExceptionFilterAttribute : ExceptionFilterAttribute
    {
        protected override Task OnCommandExceptionAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
        {
            if (exception is UIException clientError)
            {
                context.CommandResult = new PageErrorModel {
                    Message = exception.Message
                };
                context.CustomData = "Hello there";

                context.IsCommandExceptionHandled = true;
            }
            return Task.FromResult(0);
        }
    }
    public class UIException : Exception
    {
        public UIException(string message) : base(message)
        {
        }
    }
    public class SimpleExceptionFilterViewModel : DotvvmViewModelBase
    {
        [AllowStaticCommand]
        [ClientExceptionFilter]
        public static void StaticCommand()
        {
            throw new UIException("Problem!");
        }
    }
}

