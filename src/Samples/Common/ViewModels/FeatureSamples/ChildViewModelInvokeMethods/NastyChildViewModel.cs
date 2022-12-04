using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ChildViewModelInvokeMethods
{
    public class NastyChildViewModel : DotvvmViewModelBase
    {
        public int InitCount { get; set; }
        public int LoadCount { get; set; }
        public int PreRenderCount { get; set; }

        public override Task Init()
        {
            InitCount++;
            return Task.CompletedTask;
        }

        public override Task Load()
        {
            LoadCount++;
            return Task.CompletedTask;
        }

        public override Task PreRender()
        {
            PreRenderCount++;
            return Task.CompletedTask;
        }
    }
}
