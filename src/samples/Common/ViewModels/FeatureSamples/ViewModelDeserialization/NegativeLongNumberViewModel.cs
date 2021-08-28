using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ViewModelDeserialization
{
    public class NegativeLongNumberViewModel : DotvvmViewModelBase
    {
        public long Id { get; set; }
        public void Postback()
        {
            Id--;
        }
    }
}
