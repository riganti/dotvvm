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
            return TaskUtils.GetCompletedTask();
        }

        public override Task Load()
        {
            LoadCount++;
            return TaskUtils.GetCompletedTask();
        }

        public override Task PreRender()
        {
            PreRenderCount++;
            return TaskUtils.GetCompletedTask();
        }
    }
}