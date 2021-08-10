using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ChildViewModelInvokeMethods
{
    public class ChildViewModel : DotvvmViewModelBase
    {
        public int InitCount { get; set; }
        public int LoadCount { get; set; }
        public int PreRenderCount { get; set; }

        public override Task Init()
        {
            InitCount++;
            return base.Init();
        }

        public override Task Load()
        {
            LoadCount++;
            return base.Load();
        }

        public override Task PreRender()
        {
            PreRenderCount++;
            return base.PreRender();
        }
    }
}