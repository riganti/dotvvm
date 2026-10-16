using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class ClipboardTranslationViewModel : DotvvmViewModelBase
    {
        public string TextToCopy { get; set; } = "Text copied to the clipboard";
        public bool ClipboardCopied { get; set; }
    }
}
