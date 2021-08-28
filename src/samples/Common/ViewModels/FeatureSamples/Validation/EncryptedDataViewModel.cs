using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class EncryptedDataViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.EncryptData)]
        public int PrivateCount { get; set; }

        public int PublicCount { get; set; }

        public void IncreaseCount()
        {
            PublicCount = ++PrivateCount;
        }
    }
}

