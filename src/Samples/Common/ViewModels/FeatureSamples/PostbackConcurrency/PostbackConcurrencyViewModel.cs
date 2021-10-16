using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostbackConcurrency
{
    public class PostbackConcurrencyViewModel : DotvvmViewModelBase
    {
        public const int LongActionDurationInMs = 3000;

        public StateViewModel State { get; set; } = new StateViewModel();

        [FromQuery("concurrency")]
        [Bind(Direction.None)]
        public PostbackConcurrencyMode ConcurrencyMode { get; set; }

        public void LongAction()
        {
            Thread.Sleep(LongActionDurationInMs);
            State.CurrentIndex++;
            State.LastAction = "long";
        }

        public void ShortAction()
        {
            State.CurrentIndex++;
            State.LastAction = "short";
        }

        [AllowStaticCommand]
        public static StateViewModel LongAction(int currentIndex)
        {
            Thread.Sleep(LongActionDurationInMs);
            currentIndex++;
            return new StateViewModel { CurrentIndex = currentIndex, LastAction = "long" };
        }

        [AllowStaticCommand]
        public static StateViewModel ShortAction(int currentIndex)
        {
            currentIndex++;
            return new StateViewModel { CurrentIndex = currentIndex, LastAction = "short" };
        }

        public class StateViewModel
        {
            public int CurrentIndex { get; set; }

            public string LastAction { get; set; }
        }
    }
}
