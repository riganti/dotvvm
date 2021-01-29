using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomResponseProperties
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
                context.CustomResponseProperties.Add("validation-errors",new PageErrorModel {
                    Message = clientError.Message
                });
                context.CustomResponseProperties.Add("Message", "Hello there");

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
        public string TestProperty { get; set; }

        [AllowStaticCommand]
        [ClientExceptionFilter]
        public static void StaticCommand()
        {
            throw new UIException("Problem!");
        }

        [ClientExceptionFilter]
        public void Command()
        {
            throw new UIException("Problem!");
        }

        [AllowStaticCommand]
        [ClientExceptionFilter]
        public static async Task AsyncStaticCommand()
        {
            await Task.Delay(500);
            throw new UIException("Problem!");
        }

        [ClientExceptionFilter]
        public async Task AsyncCommand()
        {
            await Task.Delay(500);
            throw new UIException("Problem!");
        }

        [AllowStaticCommand]
        [ClientExceptionFilter]
        public static string StaticCommandResult()
        {
            throw new UIException("Problem!");
        }

        [ClientExceptionFilter]
        public string CommandResult()
        {
            throw new UIException("Problem!");
        }

        [AllowStaticCommand]
        [ClientExceptionFilter]
        public static async Task<string> AsyncStaticCommandResult()
        {
            await Task.Delay(500);
            throw new UIException("Problem!");
        }

        [ClientExceptionFilter]
        public async Task<string> AsyncCommandResult()
        {
            await Task.Delay(500);
            throw new UIException("Problem!");
        }
    }
}

