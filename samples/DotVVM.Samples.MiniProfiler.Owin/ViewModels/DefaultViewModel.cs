using System;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.MiniProfiler.Owin.ViewModels
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

        private static Random random = new Random();

        public void Command()
        {
            Thread.Sleep(random.Next(100, 800));
        }

        [AllowStaticCommand]
        public static void StaticCommand()
        {
            Thread.Sleep(random.Next(100, 800));
        }
    }
}