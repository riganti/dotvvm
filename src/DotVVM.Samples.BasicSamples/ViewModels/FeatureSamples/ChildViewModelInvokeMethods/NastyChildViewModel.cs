using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ChildViewModelInvokeMethods
{
    public class NastyChildViewModel : DotvvmViewModelBase
    {
        public int InitCount { get; set; }
        public int LoadCount { get; set; }
        public int PreRenderCount { get; set; }

        public async override Task Init()
        {
            InitCount++;
        }

        public async override Task Load()
        {
            LoadCount++;
        }

        public async override Task PreRender()
        {
            PreRenderCount++;
        }
    }
}