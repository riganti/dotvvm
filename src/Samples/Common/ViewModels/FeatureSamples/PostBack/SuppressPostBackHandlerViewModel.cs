using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class SuppressPostBackHandlerViewModel : DotvvmViewModelBase
    {
        public bool Condition { get; set; } = true;
        public int Counter { get; set; }

        public void ChangeCondition()
        {
            Condition = !Condition;
        }

        public void PostBack()
        {
            Counter++;
        }
    }
}
