using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Samples.MiniProfiler.AspNetCore.ViewModels
{
    public class Page2ViewModel : DefaultViewModel
    {
        public override Task PreRender()
        {
            Thread.Sleep(200);
            return base.PreRender();
        }
    }
}

