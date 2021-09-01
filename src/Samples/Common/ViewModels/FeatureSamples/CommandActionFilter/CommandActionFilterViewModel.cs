using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CommandActionFilter
{
    public class CommandActionFilterViewModel : DotvvmViewModelBase
    {
        public bool OnCommandExecuted { get; set; }
        public bool OnCommandExecuting { get; set; }

        [TestActionFilter]
        public void Method()
        {

        }
    }

    public class TestActionFilter : ActionFilterAttribute
    {
        protected override Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
        {
            ((CommandActionFilterViewModel)context.ViewModel).OnCommandExecuted = true;
            return base.OnCommandExecutedAsync(context, actionInfo, exception);
        }

        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            ((CommandActionFilterViewModel)context.ViewModel).OnCommandExecuting = true;
            return base.OnCommandExecutingAsync(context, actionInfo);
        }
    }
}

