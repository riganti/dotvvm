using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class Filter: ExceptionFilterAttribute
    {
        protected override Task OnCommandExceptionAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception ex)
        {
            if(ex is NotSupportedException)
            {
                context.CustomResponseProperties.Add("test","Test ok");
                context.IsCommandExceptionHandled = true;
            }
            return Task.FromResult(0);
        }
    }

    public class TaskBag
    {
        public ConcurrentBag<Task> Bag { get; } = new ConcurrentBag<Task>();

        public void Add(Task task) => Bag.Add(task);

        public TaskAwaiter GetAwaiter()
        {
            return Task.WhenAll(Bag.ToArray()).GetAwaiter();
        }
    }

    public class CustomAwaitableViewModel : DotvvmViewModelBase
    {
        [Filter]
        [AllowStaticCommand]
        public TaskBag Test()
        {
            var bag = new TaskBag();
            bag.Add(Task.FromResult(0));
            return bag;
        }
    }
}

