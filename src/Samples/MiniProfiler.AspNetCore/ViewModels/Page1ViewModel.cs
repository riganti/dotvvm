using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Samples.MiniProfiler.AspNetCore.ViewModels
{
    public class Page1ViewModel : DefaultViewModel
    {
        public Page1ViewModel(Models.SampleContext sampleContext) : base(sampleContext)
        {
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
    }
}