using System;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.MiniProfiler.AspNetCore.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        public string Title { get; set; }

        public DefaultViewModel()
        {
            Title = "Hello from DotVVM!";
        }

        public override Task Init()
        {
            Thread.Sleep(50);
            return base.Init();
        }

        public override Task Load()
        {
            Thread.Sleep(100);
            return base.Load();
        }

        public override Task PreRender()
        {
            Thread.Sleep(200);
            return base.PreRender();
        }

        public void Command()
        {
        }
    }
}
