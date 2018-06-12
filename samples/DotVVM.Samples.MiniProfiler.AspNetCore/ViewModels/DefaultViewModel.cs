using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.MiniProfiler.AspNetCore.Models;
using StackExchange.Profiling;

namespace DotVVM.Samples.MiniProfiler.AspNetCore.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        private readonly SampleContext _sampleContext;

        public string Title { get; set; }

        public DefaultViewModel(SampleContext sampleContext)
        {
            Title = "Hello from DotVVM!";
            _sampleContext = sampleContext;
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
            _sampleContext.Users.ToList();
            return base.PreRender();
        }

        public void Command()
        {
            _sampleContext.Users.Add(new User { Id = Guid.NewGuid(), UserName = "username" });
            _sampleContext.SaveChanges();
        }

        [AllowStaticCommand]
        public static void StaticCommand()
        {
            using (StackExchange.Profiling.MiniProfiler.Current.Step("InitUser"))
            {
                Thread.Sleep(200);

                using (StackExchange.Profiling.MiniProfiler.Current.Step("InitUser2"))
                {
                    Thread.Sleep(200);
                }
            }
        }
    }
}
