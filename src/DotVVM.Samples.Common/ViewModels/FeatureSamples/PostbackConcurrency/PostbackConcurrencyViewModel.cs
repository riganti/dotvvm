using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostbackConcurrency
{
    public class PostbackConcurrencyViewModel : DotvvmViewModelBase
    {
        public const int LongActionDurationInMs = 3000;

        public int CurrentIndex { get; set; }

        public string LastAction { get; set; }

        [FromQuery("concurrency")]
        [Bind(Direction.None)]
        public PostbackConcurrencyMode ConcurrencyMode { get; set; }

        public void LongAction()
        {
            Thread.Sleep(LongActionDurationInMs);
            CurrentIndex++;
            LastAction = "long";
        }

        public void ShortAction()
        {
            CurrentIndex++;
            LastAction = "short";
        }
    }
}
