using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.ViewModel;
using DotVVM.Tracing.MiniProfiler;
using StackExchange.Profiling;

namespace DotVVM.Samples.MiniProfiler.Owin.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        public DefaultViewModel(IMiniProfilerRequestTracer tracer)
        {
            Title = "Hello from DotVVM!";
            this.Tracer = tracer;
        }
        public string Title { get; set; }


        public override async Task Init()
        {
            Thread.Sleep(100);
            using (var timing = Tracer.Step("EVENT#2 "))
            {
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("EVENT#2.1"))
                {
                    Thread.Sleep(100);
                }
            }
            using (var timing = Tracer.Step("EVENT#4 "))
            {
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("EVENT#4.1"))
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("EVENT#4.2"))
                {
                    Thread.Sleep(100);
                }
            }
            await base.Init();
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

        private static Random random = new Random();
        private readonly IMiniProfilerRequestTracer Tracer;

        public void Command()
        {
            Thread.Sleep(random.Next(100, 800));
        }
    }
}