using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Diagnostics;

namespace DotvvmApplication1.ViewModels
{
    public class ErrorViewModel : DotvvmViewModelBase
    {
        [Bind(Direction.None)]
        public string? RequestId { get; set; }

        [Bind(Direction.None)]
        public string ExceptionType { get; set; }

        [Bind(Direction.None)]
        public string RequestPath { get; set; }


        public ErrorViewModel()
        {
        }

        public override Task Init()
        {
            var aspcontext = Context.GetAspNetCoreContext();
            var exceptionInfo = aspcontext.Features.Get<IExceptionHandlerFeature
>();
            ExceptionType = exceptionInfo.Error.GetType().Name;
            RequestId = aspcontext.TraceIdentifier;
            RequestPath = exceptionInfo.Path;
            return base.Init();
        }
    }
}
